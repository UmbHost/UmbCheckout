import { test as umbracoTest } from '@umbraco-cms/acceptance-test-helpers';
import { expect } from '@playwright/test';

/**
 * Browser console noise that is not an UmbCheckout defect.
 *
 * Keep this list SHORT and justified - every entry is a class of regression the suite can no
 * longer see. Add a comment saying why, and prefer fixing the source.
 */
const IGNORED_CONSOLE_ERRORS: RegExp[] = [
  // Chromium reports a missing favicon as a console error.
  /favicon\.ico/i,
  // Benign browser layout notice, not an application error.
  /ResizeObserver loop/i,
];

function isRelevant(message: string): boolean {
  return !IGNORED_CONSOLE_ERRORS.some((pattern) => pattern.test(message));
}

type UmbCheckoutFixtures = {
  /** Every console error and uncaught page error seen during the test. */
  consoleErrors: string[];
  /** Auto-fixture: fails the test if any unignored console error was recorded. */
  failOnConsoleErrors: void;
  /** Auto-fixture: keeps the cached backoffice login valid between tests. */
  keepLoginStateValid: void;
};

export const test = umbracoTest.extend<UmbCheckoutFixtures>({
  keepLoginStateValid: [
    async ({ umbracoApi }, use) => {
      await use();
    },
    { auto: true },
  ],

  consoleErrors: async ({ page }, use) => {
    const errors: string[] = [];

    page.on('console', (message) => {
      if (message.type() === 'error') {
        errors.push(message.text());
      }
    });
    page.on('pageerror', (error) => {
      const stack = error.stack ? `\n${error.stack}` : '';
      errors.push(`[pageerror] ${error.message}${stack}`);
    });

    await use(errors);
  },

  failOnConsoleErrors: [
    async ({ consoleErrors }, use, testInfo) => {
      await use();

      // Don't pile a second failure onto a test that already failed for a better reason.
      if (testInfo.status !== testInfo.expectedStatus) {
        return;
      }

      const relevant = consoleErrors.filter(isRelevant);
      expect(relevant, `Unexpected browser console errors:\n${relevant.join('\n')}`).toEqual([]);
    },
    { auto: true },
  ],
});

export { expect };
