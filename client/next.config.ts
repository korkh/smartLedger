import withFlowbiteReact from "flowbite-react/plugin/nextjs";
import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  logging: {
    fetches: {
      fullUrl: true,
    },
  },
  images: {
    remotePatterns: [
      { hostname: "scu-bucket-3.oss-eu-central-1.aliyuncs.com" },
      { hostname: "cdn.shopify.com" },
    ].map((pattern) => ({
      protocol: "https",
      port: "",
      pathname: "/**",
      ...pattern,
    })),
  },
  // MUST BE SET TO "STANDALONE"
  output: "standalone",
};

export default withFlowbiteReact(nextConfig);
