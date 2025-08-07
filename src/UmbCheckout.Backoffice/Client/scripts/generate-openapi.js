import fetch from 'node-fetch';
import chalk from 'chalk';
import { createClient, defaultPlugins } from '@hey-api/openapi-ts';

console.log(chalk.green("Generating OpenAPI client..."));

// Get input arguments
const swaggerUrl = process.argv[2];
const outputDir = process.argv[3];

// Validate input
if (!swaggerUrl || !outputDir) {
  console.error(chalk.red(`ERROR: Missing required arguments`));
  console.error(`Usage: node generate-openapi.js ${chalk.yellow('<swaggerUrl> <outputDir>')}`);
  console.error(`Example: node generate-openapi.js ${chalk.yellow('https://localhost:44331/swagger/backoffice/swagger.json src/api/backoffice')}`);
  process.exit(1);
}

// Ignore self-signed certificates (dev mode)
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

// Fetch and generate client
console.log("Ensure your Umbraco instance is running");
console.log(`Fetching OpenAPI definition from ${chalk.yellow(swaggerUrl)}`);

fetch(swaggerUrl).then(async (response) => {
  if (!response.ok) {
    console.error(chalk.red(`ERROR: OpenAPI spec returned a non-OK response: ${response.status} ${response.statusText}`));
    console.error(`Check that the Umbraco instance is running and the URL is correct`);
    return;
  }

  console.log(`✅ OpenAPI spec fetched successfully`);
  console.log(`⚙️  Generating client in ${chalk.yellow(outputDir)}`);

  await createClient({
    input: swaggerUrl,
    output: outputDir,
    plugins: [
      ...defaultPlugins,
      '@hey-api/client-fetch',
      {
        name: '@hey-api/typescript',
        enums: 'typescript',
      },
      {
        name: '@hey-api/sdk',
        asClass: true,
      },
    ],
  });

  console.log(chalk.green(`✅ TypeScript client generated at ${outputDir}`));

}).catch(error => {
  console.error(`ERROR: Failed to connect to the OpenAPI spec: ${chalk.red(error.message)}`);
  console.error(`Check the URL and ensure your Umbraco instance is running`);
});
