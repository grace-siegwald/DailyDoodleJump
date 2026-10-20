// Drives the KNI game loop. Before each repaint, the browser calls tick(),
// which asks .NET (Home.razor's TickDotNet) to run one Update + Draw.
// Adapted from KNI's Blazor template (index.html in BlazorGL.NetCore.CSharp).
(function () {
  let page = null;
  let frameId = 0;

  // The canvas the game draws into. It is made once and kept for as long as the app is
  // open: KNI binds to it (and to its WebGL context) when the game starts and never looks
  // it up again, so leaving the page takes it off the page instead of throwing it away.
  let canvas = null;

  // The box on the page the canvas sits in, and the size of the game's play area.
  let holder = null;
  let holderObserver = null;
  let playWidth = 0;
  let playHeight = 0;

  // Last measured size for the canvas, so tick() doesn't read the layout every frame.
  let width = 0;
  let height = 0;

  // The biggest play-area-shaped box that fits the holder. Keeping the canvas exactly
  // that shape leaves the game no letterboxing to draw inside its own box; CSS then
  // stretches those few leftover pixels to the holder's edges.
  function measureHolder() {
    const scale = Math.min(holder.clientWidth / playWidth, holder.clientHeight / playHeight);
    width = Math.round(playWidth * scale);
    height = Math.round(playHeight * scale);
  }

  // KNI stretches the canvas to the whole browser window whenever the window resizes.
  // The game lives in a box on the page, so put it back to the size measured above;
  // the game then follows the canvas (see DailyDoodleGame.UpdateScreenFit).
  function fitCanvasToHolder() {
    if (canvas.width !== width || canvas.height !== height) {
      canvas.width = width;
      canvas.height = height;
    }
  }

  function tick() {
    if (!page) {
      return;
    }
    fitCanvasToHolder();
    page.invokeMethod('TickDotNet');
    frameId = window.requestAnimationFrame(tick);
  }

  // Called by Home.razor once the holder is on the page.
  window.initRenderJS = (dotNetPage, virtualWidth, virtualHeight) => {
    page = dotNetPage;
    playWidth = virtualWidth;
    playHeight = virtualHeight;

    holder = document.getElementById('canvasHolder');

    if (canvas === null) {
      canvas = document.createElement('canvas');
      canvas.id = 'theCanvas'; // KNI looks the canvas up by this id.
      canvas.addEventListener('contextmenu', (e) => e.preventDefault());
    }

    holder.appendChild(canvas);
    measureHolder();
    fitCanvasToHolder();

    // The holder is sized in % and vh, so it changes with the browser window.
    holderObserver = new ResizeObserver(measureHolder);
    holderObserver.observe(holder);

    frameId = window.requestAnimationFrame(tick);
  };

  // Called by Home.razor when the page goes away, so .NET isn't called on a removed page.
  window.stopRenderJS = () => {
    page = null;
    window.cancelAnimationFrame(frameId);

    if (holderObserver !== null) {
      holderObserver.disconnect();
      holderObserver = null;
    }

    // Take the canvas off the page but keep it, so the game can carry on when you come back.
    if (canvas !== null) {
      canvas.remove();
    }
  };

  // Stop the arrow keys and space bar from scrolling the page while playing.
  window.addEventListener('keydown', (e) => {
    if (page && ['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', ' '].includes(e.key)) {
      e.preventDefault();
    }
  });
})();
