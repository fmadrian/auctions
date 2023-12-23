# 1. Get node and store it into the 'deps' build stage.
FROM node:18-alpine AS deps
# 2. Install libc6-compat
RUN apk add --no-cache libc6-compat
# 3. Switch to the 'app' directory (in service) and copy to the 'app' directory.
WORKDIR /app
COPY frontend/web-app/package*.json ./
# 4. Install everything but development packages.
RUN  npm install --omit-dev
# 5. Get node for the 'builder' stage.
FROM node:18-alpine AS builder
# 6. Copy files from "node_modules" directory in the 'deps' stage into "node_modules" in service.
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
# 7. Copy all the files into the 'app' directory in service.
COPY frontend/web-app ./

# 8. Disable telemetry.
ENV NEXT_TELEMETRY_DISABLED 1
# 9. Build the app.
RUN npm run build

# 10. Create production image, copy all files and run next
FROM node:18-alpine AS runner
WORKDIR /app
# Set environment to production.
ENV NODE_ENV production

ENV NEXT_TELEMETRY_DISABLED 1
# Add users and groups
RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs
# Copy public assets, package.json and next.config.js
COPY --from=builder /app/public ./public
COPY --from=builder /app/package.json ./package.json
COPY --from=builder /app/next.config.js ./next.config.js
# Copy standalone image (you have to create it first) and static assets..
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static
# Set user, expose port, define port as environment variable
USER nextjs

EXPOSE 3000

ENV PORT 3000
# Set entry point.
CMD ["node", "server.js"]