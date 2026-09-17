import { defineConfig, devices } from '@playwright/test';
import dotenv from 'dotenv';

dotenv.config({ path: '.env' });

const storageState = process.env.STORAGE_STATE_PATH ?? 'playwright/.auth/user.json';
const baseURL = process.env.URL ?? 'https://localhost:44387';

export default defineConfig({
  testDir: './tests',
  timeout: 60 * 1000,
  expect: { timeout: 10 * 1000 },

  fullyParallel: false,
  workers: 1,

  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,

  reporter: process.env.CI
    ? [['github'], ['html', { open: 'never' }]]
    : [['list'], ['html', { open: 'never' }]],

  use: {
    baseURL,
    ignoreHTTPSErrors: true,
    testIdAttribute: 'data-mark',
    trace: 'on-first-retry',
    video: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'setup',
      testMatch: /auth\.setup\.ts/,
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'backoffice',
      testDir: './tests/backoffice',
      dependencies: ['setup'],
      use: {
        ...devices['Desktop Chrome'],
        storageState,
      },
    },
    {
      name: 'storefront',
      testDir: './tests/storefront',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
