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
window.dailyDoodleProfile = {
  load: function () {
    try {
      const profile = JSON.parse(
        localStorage.getItem("dailyDoodleJump.profile") || "{}"
      );

      return {
        nickname: typeof profile?.nickname === "string"
          ? profile.nickname.trim().slice(0, 20) || "Player"
          : "Player",
        avatar: ["star", "rocket", "frog", "bear", "elephant", "car", "balloon"].includes(profile?.avatar)
          ? profile.avatar
          : "star"
      };
    } catch {
      return { nickname: "Player", avatar: "star" };
    }
  },

  save: function (nickname, avatar) {
    try {
      localStorage.setItem("dailyDoodleJump.profile", JSON.stringify({
        nickname: nickname,
        avatar: avatar
      }));
      return true;
    } catch {
      return false;
    }
  }
};
