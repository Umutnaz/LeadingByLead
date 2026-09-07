import { test, expect } from '@playwright/test';

test.setTimeout(120000);

const base = 'http://localhost:5005';

test('host sees effect messages and can advance', async ({ page }) => {
  // Create a session with one question via API
  const sessionPayload = {
    Characters: [],
    Questions: [
      {
        RequiredSelections: 1,
        Title: 'E2E Q',
        Description: 'E2E',
        AnswerOptions: [
          {
            Id: 'opt1',
            Text: 'Option 1',
            CharacterEffects: [
              {
                CharacterId: '',
                Changes: [{ StatName: 'TjenesteMotivation', Amount: 1 }],
                Reaction: 'Test reaction'
              }
            ]
          }
        ]
      }
    ]
  };

  await fetch(`${base}/api/v1/gamesessions`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(sessionPayload) });

  // join a player
  await fetch(`${base}/api/v1/gamesessions/1/join`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ Name: 'e2e-player', Id: 'e2e-p1' }) });

  // start the session
  await fetch(`${base}/api/v1/gamesessions/1/start`, { method: 'POST' });

  // Set localStorage to Host and session
  await page.addInitScript(() => {
    localStorage.setItem('lbl_player_role', 'Host');
    localStorage.setItem('lbl_session_current', '1');
  });

  await page.goto(`${base}/gameviewer-host`, { waitUntil: 'networkidle' });

  // Wait for page to load (Blazor WASM can take a while)
  await page.waitForSelector('button.next-button', { timeout: 60000 });

  const button = page.locator('button.next-button');

  // Click twice (confirmation then show effects)
  await button.click();
  await button.click();

  // Effects should be visible under .effect-messages
  const effect = page.locator('.effect-messages p');
  await expect(effect).toHaveText(/Test reaction/);

  // Click once to advance phase (server moves to Results)
  await button.click();

  // Optionally advance server; E2E focus is UX verification of effect messages
  // await fetch(`${base}/api/v1/gamesessions/1/next`, { method: 'POST' });

});
