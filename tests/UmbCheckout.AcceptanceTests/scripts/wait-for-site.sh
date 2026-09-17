#!/usr/bin/env bash
# Waits until the starter kit site is serving the backoffice, the UmbCheckout bundles and the storefront,
# and uSync has finished its startup import.
#
# Usage: wait-for-site.sh <site url> <publish folder> <pid file> <expected number of completed uSync startups>
#
# The uSync startup count is how the second boot is told apart from the first: both boots write to the same
# Umbraco log file, and the site already answers requests while uSync is still importing in the background.
set -uo pipefail

site_url=$1
publish_dir=$2
pid_file=$3
expected_usync_startups=$4
timeout_seconds=${WAIT_TIMEOUT_SECONDS:-600}

umbraco_logs() {
  echo "--- site.log (stdout/stderr) ---"
  cat site.log 2>/dev/null || echo "(no site.log)"
  echo "--- Umbraco trace logs ---"
  if ls "$publish_dir"/umbraco/Logs/*.json >/dev/null 2>&1; then
    tail -n 40 "$publish_dir"/umbraco/Logs/*.json
  else
    echo "(no Umbraco log files - the host never got far enough to create them)"
  fi
}

probe() {
  curl -skf -o /dev/null "$site_url$1" && echo yes || echo no
}

deadline=$((SECONDS + timeout_seconds))
last_report=0
while [ $SECONDS -lt $deadline ]; do
  # Fail fast if the host died instead of burning the whole budget on a process that is gone
  if ! kill -0 "$(cat "$pid_file")" 2>/dev/null; then
    echo "::error::The site process exited during startup."
    umbraco_logs
    exit 1
  fi

  backoffice_ok=$(probe /umbraco)
  bundles_ok=$( [ "$(probe /App_Plugins/UmbCheckout/umbcheckout.js)" = yes ] && [ "$(probe /App_Plugins/UmbCheckout.Stripe/umbcheckout-stripe.js)" = yes ] && echo yes || echo no)
  storefront_ok=$(probe /basket/)
  usync_startups=$(cat "$publish_dir"/umbraco/Logs/*.json 2>/dev/null | grep -c "uSync: Startup Complete")

  if [ "$backoffice_ok" = yes ] && [ "$bundles_ok" = yes ] && [ "$storefront_ok" = yes ] && [ "$usync_startups" -ge "$expected_usync_startups" ]; then
    echo "Site is ready after ${SECONDS}s."
    exit 0
  fi

  if [ $((SECONDS - last_report)) -ge 30 ]; then
    last_report=$SECONDS
    echo "[${SECONDS}s] backoffice=$backoffice_ok bundles=$bundles_ok storefront=$storefront_ok usyncStartups=$usync_startups/$expected_usync_startups"
  fi
  sleep 5
done

echo "::error::Site did not become ready within ${timeout_seconds}s."
umbraco_logs
exit 1
