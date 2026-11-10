window.dailyDoodleTheme = {
  get: function () {
    return document.documentElement.dataset.theme === "dark";
  },

  set: function (isDark) {
    const theme = isDark ? "dark" : "light";
    document.documentElement.dataset.theme = theme;

    try {
      localStorage.setItem("dailyDoodleJump.theme", theme);
    } catch {
      // The theme still works if browser storage is unavailable.
    }
  }
};

try {
  const savedTheme = localStorage.getItem("dailyDoodleJump.theme");
  document.documentElement.dataset.theme =
    savedTheme === "dark" ? "dark" : "light";
} catch {
  document.documentElement.dataset.theme = "light";
}

