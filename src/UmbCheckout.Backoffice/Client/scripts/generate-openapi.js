import fetch from "node-fetch";
import chalk from "chalk";
import { createClient, defaultPlugins } from "@hey-api/openapi-ts";

const targets = process.argv.slice(2).map((arg) => {
  const [url, output] = arg.split("=");
  return { url, output: output ?? "src/api" };
});

if (targets.length === 0) {
  console.error(chalk.red("ERROR: Missing URL to OpenAPI spec (usage: url=outputDir ...)"));
  process.exit(1);
}

// Needed to ignore self-signed certificates from running Umbraco on https on localhost
process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";

for (const { url, output } of targets) {
  console.log(`Fetching OpenAPI definition from ${chalk.yellow(url)}`);
  const response = await fetch(url);
  if (!response.ok) {
    console.error(chalk.red(`ERROR: ${response.status} ${response.statusText} from ${url}. Is the site running?`));
    process.exit(1);
  }

  await createClient({
    input: url,
    output,
    plugins: [
      ...defaultPlugins,
      { name: "@hey-api/client-fetch", throwOnError: true },
      { name: "@hey-api/sdk", asClass: true, classNameBuilder: "{{name}}Service" },
    ],
  });
  console.log(chalk.green(`Generated ${output}`));
}
