import { c as t } from "./client.gen-CGf2xBln.js";
class c {
  static getConfiguration(e) {
    return (e?.client ?? t).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/management/api/v1/umbcheckout/configuration",
      ...e
    });
  }
  static updateConfiguration(e) {
    return (e?.client ?? t).put({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/management/api/v1/umbcheckout/configuration",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e?.headers
      }
    });
  }
  static getApplicationUrlStatus(e) {
    return (e?.client ?? t).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/management/api/v1/umbcheckout/configuration/application-url",
      ...e
    });
  }
}
export {
  c as C
};
//# sourceMappingURL=sdk.gen-bktVcCtQ.js.map
