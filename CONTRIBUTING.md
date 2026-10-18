# Working on Daily Doodle Jump

This is the everyday workflow for everyone on the team. The short version:
**never push straight to `main`. Work on a branch, check it, open a pull request.**

Anything merged into `main` deploys to the live site on GitHub Pages automatically,
so `main` should always work.

---

## One-time setup

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download) and [Git](https://git-scm.com/downloads).
2. Clone the repo and check it builds:

   ```
   git clone https://github.com/grace-siegwald/DailyDoodleJump.git
   cd DailyDoodleJump
   dotnet build
   ```

---

## 1. Start a branch

Always start from an up-to-date `main`:

```
git switch main
git pull
git switch -c feature/short-description
```

Name branches after what you're doing, for example:

- `feature/platform-generation`
- `fix/player-falls-through-floor`
- `docs/update-readme`

One branch = one task. If you start working on something unrelated, make a new branch.

---

## 2. Run the game while you work

```
dotnet watch --project src/DailyDoodle.Web
```

This opens the site in your browser (the address is printed in the terminal, e.g.
`http://localhost:5xxx`). When you save a `.razor`, `.cs`, or `.css` file the page
updates on its own. If something doesn't update, press **Ctrl+R** in the terminal
to restart.

Play it like a real user would. Useful browser dev tools (**F12**):

- **Console** – errors and anything you print with `Console.WriteLine`.
- **Application → Local Storage** – saved data like streaks; delete it to act like a new player.
- **Device toolbar (Ctrl+Shift+M)** – check it works at phone sizes.

**Tip:** use relative paths for images and links (`images/doodle.png`, not
`/images/doodle.png`). This includes images the game loads, such as
`LoadTexture("images/player.png")`. The live site runs at `/DailyDoodleJump/`, so paths starting
with `/` work locally but break on GitHub Pages.

**Where code goes:**

| Project | What goes in it |
|---|---|
| `src/DailyDoodle.Core` | Game logic: level generation, physics, scoring, streaks. No UI code. |
| `src/DailyDoodle.Kni` | The KNI game class: reads input, calls into `Core`, draws sprites. No game rules. |
| `src/DailyDoodle.UI` | Reusable Razor components. |
| `src/DailyDoodle.Web` | The app itself: pages, layout, startup. |
| `tests/DailyDoodle.Core.Tests` | Unit tests for `Core`. |

---

## 3. Commit often

Commit every time you reach a small working step, not once at the end. Small
commits are easier to review and easier to undo.

```
git add .
git commit -m "Add platform spacing to level generator"
```

Good commit messages say **what** changed, in a few words, starting with a verb:

- ✅ `Add jump sound effect`
- ✅ `Fix streak resetting at midnight`
- ❌ `stuff`, `changes`, `asdf`, `final final v2`

Push your branch now and then so your work is backed up on GitHub:

```
git push -u origin feature/short-description   # first time
git push                                        # after that
```

### Keeping your branch up to date

If `main` has changed while you've been working, bring those changes into your branch:

```
git switch main
git pull
git switch feature/short-description
git merge main
```

If Git reports a **merge conflict**, open the listed files, choose which version to
keep (your editor will highlight the conflicting parts), save, then:

```
git add .
git commit
```

Ask in the team chat if you're unsure. Don't delete someone else's code to make a conflict go away.

---

## 4. Check your work before a pull request

Run these three commands from the repo root. All three should pass.

```
dotnet build -c Release
dotnet format --verify-no-changes
dotnet test
```

| Command | What it checks |
|---|---|
| `dotnet build -c Release` | Everything compiles, using the same Release build the live site uses. |
| `dotnet format --verify-no-changes` | Code follows the team style in `.editorconfig`. Run `dotnet format` (without the flag) to auto-fix most issues; naming issues need fixing by hand. |
| `dotnet test` | All unit tests still pass. Add tests for new logic in `Core`. |

Pull requests don't run these checks automatically yet, so **you** are the
safety net. Please don't skip them.

---

## 5. Open a pull request

1. Push your latest commits: `git push`
2. Go to the repo on GitHub. You'll see a **Compare & pull request** button; click it.
   (Or go to **Pull requests → New pull request** and pick your branch.)
3. Make sure it says **base: `main`** ← **compare: your branch**.
4. Fill it in:
   - **Title:** what the PR does, e.g. *Add platform generation*.
   - **Description:** what you changed, how to try it, and screenshots if it's visual.
   - If it finishes an issue, write `Closes #12` (with the issue number) so the issue closes automatically when merged.
5. Request a reviewer on the right-hand side.

### Reviewing and merging

- At least one other teammate reviews before merging.
- If the reviewer asks for changes, make them on the same branch, commit, and `git push`. The PR updates on its own.
- Once approved, click **Merge pull request**. The site deploys a minute or two later. You can watch it under the **Actions** tab.

### After merging

Clean up and get ready for your next task:

```
git switch main
git pull
git branch -d feature/short-description
```

Then start again at step 1.

---

## Quick reference

```
# start
git switch main && git pull
git switch -c feature/my-task

# work
dotnet watch --project src/DailyDoodle.Web
git add . && git commit -m "Describe the change"
git push

# before the PR
dotnet build -c Release
dotnet format --verify-no-changes
dotnet test
```
