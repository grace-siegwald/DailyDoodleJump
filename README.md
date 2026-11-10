# Daily Doodle Jump

[![Deploy to GitHub Pages](https://github.com/grace-siegwald/DailyDoodleJump/actions/workflows/build-and-deploy.yml/badge.svg)](https://github.com/grace-siegwald/DailyDoodleJump/actions/workflows/build-and-deploy.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

A daily version of Doodle Jump that runs in the browser. Every day there's one new level, the same
for everyone, and the goal is to climb to its finish line and keep a streak going by finishing a
level every day. It's for anyone who likes a quick daily game, like Wordle but with jumping.

**Play it:** <https://grace-siegwald.github.io/DailyDoodleJump/>

## Features

- **A new level every day.** Each date's level is generated from that date, so everyone playing on the
  same day gets the same platforms, with no server needed.
- **A finish line to reach.** Climb 1,000 to win the day. The win screen tells you to come back
  tomorrow, or to try a past level.
- **Streaks and a calendar.** Finished days get a check mark on the calendar, and finishing levels on the
  day they come out builds your streak.
- **Past levels.** Pick any earlier day on the calendar to play the level you missed.
- **Live height.** The game shows how high you are right now, and the highest you've got today.
- **Saved on your device.** Progress is kept in the browser's local storage. No accounts, and no data
  leaves your computer.
- **Plays on phones too.** Hold the left or right half of the game to steer.

## How to play

| Action | Keyboard | Phone |
|---|---|---|
| Move left / right | Left / Right arrows, or A / D | Hold the left / right half of the game |
| Try again after falling | Space or Enter | Tap the game |

You bounce automatically whenever you land on a platform. Platforms come in four kinds:

- **Normal:** bounces you up.
- **Moving:** slides side to side.
- **Breakable:** breaks when you land on it, so you keep falling.
- **Spring:** launches you nearly a whole screen up.

Leave one side of the screen and you come back on the other. Fall off the bottom, or touch a
monster, and the run is over.

## Built with

- [.NET 10](https://dotnet.microsoft.com/) and C#: the whole app, game included.
- [Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/): the web pages (home, calendar, library,
  settings). Runs entirely in the browser.
- [KNI](https://github.com/kniEngine/kni) 4.3.9001: an XNA/MonoGame-style game framework that draws the
  game with WebGL in the browser.
- [xUnit](https://xunit.net/) 2.9: unit tests.
- [GitHub Actions](https://docs.github.com/actions) and [GitHub Pages](https://pages.github.com/): build
  and host the site on every push to `main`.

## Getting started

### Prerequisites

- The [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), version 10.0.402 or newer
  (pinned in `global.json`).
- [Git](https://git-scm.com/downloads).

Check they're installed:

```bash
dotnet --version   # 10.0.402 or newer
git --version
```

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/grace-siegwald/DailyDoodleJump.git
   cd DailyDoodleJump
   ```

2. Restore the packages and build:

   ```bash
   dotnet build
   ```

### Usage

Run the site locally:

```bash
dotnet watch --project src/DailyDoodle.Web
```

The terminal prints a local address (for example `http://localhost:5xxx`) and opens it in your
browser. Today's level starts on the home page. To open a particular day's level, use the calendar,
or add the date to the address: `http://localhost:5xxx/?date=2026-11-03`. A date that can't be read,
or one in the future, opens today's level instead.

To start over as a brand-new player, open the browser's dev tools (F12), go to
**Application → Local Storage**, and delete `dailyDoodleJump.progress`.

## Running tests

```bash
dotnet test
```

The tests cover the game rules, which have no browser code in them: level generation (same seed gives
the same level, every gap can be jumped), platforms and landing, the finish line, live height, saved
progress and streaks (including damaged save data), and reading dates from links.

Before opening a pull request, also run:

```bash
dotnet build -c Release
dotnet format --verify-no-changes
```

## Project structure

```
src/
  DailyDoodle.Core/      The game. Rules in plain classes with no browser types, so they can be tested:
    GameWorld.cs           movement, landing, scrolling, finish line, game over
    LevelGeneration/       seeded daily levels (DailySeed, LevelGenerator, LevelDate)
    Progress/              saved results and streaks (PlayerProgress)
    DailyDoodleGame.cs     the KNI game: reads input and draws the world
  DailyDoodle.UI/        Reusable Razor components: calendar days, week strip, month calendar
  DailyDoodle.Web/       The site: pages, layout, startup, localStorage (Services/ProgressStore.cs)
tests/
  DailyDoodle.Core.Tests/  xUnit tests for DailyDoodle.Core
art/                     Full-size drawings and the script that prepares them for the game
.github/workflows/       Builds and deploys the site to GitHub Pages
```

How a frame runs: `wwwroot/js/game-loop.js` asks the browser for a frame, and `Pages/Home.razor` passes it
to `DailyDoodleGame`, which moves the `GameWorld` forward and draws it. Home then saves any finished run
and updates the height shown above the game.

## Known limitations

- Progress is saved per browser. Clearing site data, or switching devices, starts you over.
- Days follow UTC, so a new level comes out at midnight UTC, not your local midnight.
- The Settings page is a placeholder.

## Contributing

Issues and ideas go in the [issue tracker](https://github.com/grace-siegwald/DailyDoodleJump/issues).
To contribute code, branch from `main`, check your work, and open a pull request. The full workflow is in
[CONTRIBUTING.md](CONTRIBUTING.md). Anything merged into `main` deploys to the live site.

## License

Distributed under the MIT License. See [LICENSE](LICENSE).

## Acknowledgments

- Built by the Daily Doodle Jump team for Programming III at Columbia College Chicago:
  [grace-siegwald](https://github.com/grace-siegwald), [UnaRoberts](https://github.com/UnaRoberts),
  [Yunjong123](https://github.com/Yunjong123), [cbalder0929](https://github.com/cbalder0929) and
  [TameZombie510](https://github.com/TameZombie510).
- Inspired by [Doodle Jump](https://en.wikipedia.org/wiki/Doodle_Jump) by Lima Sky. This is a fan-made
  student project with original art and code, not affiliated with Lima Sky.
- Fonts: [Kalam](https://fonts.google.com/specimen/Kalam) by Indian Type Foundry and
  [Atkinson Hyperlegible](https://www.brailleinstitute.org/freefont/) by the Braille Institute, both under the
  SIL Open Font License (copies in `src/DailyDoodle.Web/wwwroot/fonts`).
- [KNI](https://github.com/kniEngine/kni) for running an XNA-style game in Blazor; the frame loop in
  `game-loop.js` is adapted from KNI's Blazor template.

## Contributors
- Una Roberts: Created game art, helped create platform and enemy functionality and worked on end screen.
- Grace Siegwald: Built Blazor scaffolding, managed repository, designed game and implemented game functionality.
- Yunjong Heo: Built the calendar page (Calendar tab, week strip, and month calendar) and its reusable components (CalendarDay, WeekStrip, MonthCalendar).
- Mia Graham: Added persistent dark mode and player statistics to Settings, clarified browser data storage, and researched account and storage options for player profiles and progress.
- Max Polancic: Created random level generation prototype and worked on the random level generation. 

### AI use

Parts of this project were written with help from Claude (Anthropic): the calendar-to-level link,
the finish line and win screen, saved progress and streaks, the height display, the enemy balance
changes, the notebook-style redesign, and this README.
Each of those commits is marked `Co-Authored-By: Claude`. The team reviewed and tested that work.


