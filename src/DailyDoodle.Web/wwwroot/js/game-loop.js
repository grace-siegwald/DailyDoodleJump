// Drives the KNI game loop. Before each repaint, the browser calls tick(),
// which asks .NET (Home.razor's TickDotNet) to run one Update + Draw.
// Adapted from KNI's Blazor template (index.html in BlazorGL.NetCore.CSharp).
(function () {
  let page = null;
  let frameId = 0;

  function tick() {
    if (!page) {
      return;
    }
    page.invokeMethod('TickDotNet');
    frameId = window.requestAnimationFrame(tick);
  }

  // Called by Home.razor once the canvas is on the page.
  window.initRenderJS = (dotNetPage) => {
    page = dotNetPage;

    // KNI looks the canvas up by this id. It starts at the holder's size;
    // after that, KNI resizes it to the window whenever the window resizes.
    const canvas = document.getElementById('theCanvas');
    const holder = document.getElementById('canvasHolder');
    canvas.width = holder.clientWidth;
    canvas.height = holder.clientHeight;
    canvas.addEventListener('contextmenu', (e) => e.preventDefault());

    frameId = window.requestAnimationFrame(tick);
  };

  // Called by Home.razor when the page goes away, so .NET isn't called on a removed page.
  window.stopRenderJS = () => {
    page = null;
    window.cancelAnimationFrame(frameId);
  };

  // Stop the arrow keys and space bar from scrolling the page while playing.
  window.addEventListener('keydown', (e) => {
    if (page && ['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', ' '].includes(e.key)) {
      e.preventDefault();
    }
  });
})();
