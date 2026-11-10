# Player Data and Account Research

Prepared by Mia for Daily Doodle Jump.

## Purpose

Compare ways to save player profiles and daily progress for a game hosted on GitHub Pages. GitHub Pages hosts the static application. So an external service would be needed for hosted accounts and shared database storage.

## Current implementation

The game already saves progress using browser localStorage.

- `PlayerProgress.cs` manages daily results and calculates streaks.
- `ProgressStore.cs` loads and saves results under `dailyDoodleJump.progress`.
- Settings currently shows a generic player profile. Sign-in is unavailable.

### Saved player data

| Field | Meaning |
|---|---|
| version | Save format version |
| days | Results indexed by level date |
| best | Highest recorded height for that level |
| finished | Whether the player completed that level |
| onTheDay | Whether the player completed it on its release date |

Streaks are calculated from consecutive on-time completions. Completing an older level fills in the calendar but does not rebuild a streak.

## Options compared

| Option | Accounts | Progress storage | Main tradeoff |
|---|---|---|---|
| Browser localStorage | No hosted accounts | On the current browser | Simple and already implemented, but no automatic syncing between devices |
| Firebase Authentication + Cloud Firestore | Yes | Hosted document database | Supports accounts and syncing, but requires service configuration, integration, and database security rules |
| Supabase Auth + Postgres | Yes | Hosted relational database | Fits structured profiles and daily results, but requires integration, database design, and Row Level Security policies |
| Auth0 + separate game database | Yes | Requires a separate solution for game results | Provides authentication, but adds another service when the game also needs progress storage |

All three hosted approaches can work with a static frontend, so they still require configuration and access controls.

## Recommendation

Keep browser localStorage for the current release.

It already supports the core daily game without requiring registration or a new hosted dependency. This allows the team to focus on verifying existing functionality.

The limitations should be clear to players:

- Progress stays in the same browser and browser profile.
- Clearing site data can remove saved progress.
- Progress does not automatically transfer between devices.
- If storage is blocked or full, the current implementation keeps progress in memory, so new progress may disappear when the page closes.
- Locally stored results can be edited by the player and should not be treated as verified leaderboard scores.

If accounts and syncing become requirements, I recommend evaluating Supabase first. Its authentication and relational database would allow player profiles and daily results to be managed together.

## Proposed future player data structure
(This is just a design proposal)

| Record | Suggested fields |
|---|---|
| Player profile | user ID, display name, avatar ID |
| Daily result | user ID, level date, best height, completed, completed on release date |

Each daily result should be unique to a player and level date. Streaks can be derived from daily results instead of being stored separately.

Only collect information needed for the game. Passwords should be handled by the authentication provider and never stored in a player profile or GitHub repository.

For hosted storage, access policies must restrict players to their own records. Supabase secret/service-role keys must never be included in the browser application. Competitive scores would also need trusted validation; authentication alone does not prove a score is legitimate.

## Suggested release refinement

Use the existing Settings page to explain that the player is playing locally and that progress is saved in this browser. Keep its appearance consistent with the game's notebook theme.

## Sources

- [GitHub Pages overview](https://docs.github.com/en/pages/getting-started-with-github-pages/what-is-github-pages)
- [MDN: localStorage](https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage)
- [Firebase: Authentication and security rules](https://firebase.google.com/docs/rules/rules-and-auth)
- [Firebase: Cloud Firestore security rules](https://firebase.google.com/docs/firestore/security/get-started)
- [Supabase: Securing your data](https://supabase.com/docs/guides/database/secure-data)
- [Supabase: Row Level Security](https://supabase.com/docs/guides/database/postgres/row-level-security)
- [Auth0: Single-page applications](https://auth0.com/docs/get-started/applications/single-page-web-apps)

## AI assistance

Prepared with assistance from ChatGPT for research comparison and structure refinement.
