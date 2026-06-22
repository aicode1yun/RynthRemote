// HD video mode for RynthRemote. recvonly => no getUserMedia, so no camera/mic permission.
//
// IMPORTANT: the offer/answer SDP exchange is done by C# (native HttpClient), NOT by a fetch() here.
// iOS WKWebView blocks an insecure http fetch from web content as "active mixed content" (the agent is
// http:// over Tailscale), so a JS fetch never leaves the phone. C# calls createOffer() to get the SDP,
// POSTs it natively, then calls applyAnswer() with the agent's reply. The WebRTC media itself is
// peer-to-peer (DTLS/SRTP over Tailscale) and unaffected by the WebView's http rules.
window.rynthHd = {
  conns: {},

  // Create a recvonly peer, return the local offer SDP (after ICE gathering) for C# to POST.
  async createOffer(videoId, pid) {
    try {
      this.stop(pid);
      const pc = new RTCPeerConnection();              // no ICE servers => host candidate over Tailscale
      this.conns[pid] = { pc, videoId };
      pc.addTransceiver('video', { direction: 'recvonly' });
      pc.ontrack = (e) => {
        const v = document.getElementById(videoId);
        if (v) { v.srcObject = e.streams[0]; v.play().catch(() => {}); }
      };
      const offer = await pc.createOffer();
      await pc.setLocalDescription(offer);
      await new Promise((res) => {
        if (pc.iceGatheringState === 'complete') return res();
        const check = () => { if (pc.iceGatheringState === 'complete') { pc.removeEventListener('icegatheringstatechange', check); res(); } };
        pc.addEventListener('icegatheringstatechange', check);
        setTimeout(res, 1200);
      });
      return pc.localDescription ? pc.localDescription.sdp : null;
    } catch (e) {
      this.stop(pid);
      return null;
    }
  },

  // Apply the agent's answer SDP (from C#). Returns false if the peer is gone or it errors.
  async applyAnswer(pid, answerSdp) {
    const c = this.conns[pid];
    if (!c || !answerSdp) return false;
    try { await c.pc.setRemoteDescription({ type: 'answer', sdp: answerSdp }); return true; }
    catch (e) { this.stop(pid); return false; }
  },

  // Close the local peer (C# separately tells the agent to stop encoding).
  stop(pid) {
    const c = this.conns[pid];
    if (c) { try { c.pc.close(); } catch (e) {} delete this.conns[pid]; }
  },
};
