/** @type {import('next').NextConfig} */
const nextConfig = {
  // 2. Server actions are enabled by default.
  // 1. Configure domains where we can retrieve images from.
  images: {
    // images.domains is deprecated
    // https://nextjs.org/docs/app/api-reference/components/image#remotepatterns
    remotePatterns: [
      {
        protocol: "https",
        hostname: "cdn.pixabay.com",
        pathname: "**",
      },
    ],
  },
  output: "standalone",
};

module.exports = nextConfig;
