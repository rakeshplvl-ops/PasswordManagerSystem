# ==========================================
# Stage 1: Build (Node 24 Alpine)
# ==========================================
FROM node:24-alpine AS build
WORKDIR /app

# Copy dependency manifests
COPY package*.json ./
RUN npm ci

# Copy source and compile production bundle
COPY . .
RUN npm run build

# ==========================================
# Stage 2: Runtime (Nginx Alpine)
# ==========================================
FROM nginx:alpine AS runtime

# Copy build artifacts to Nginx docroot
COPY --from=build /app/dist /usr/share/nginx/html

# Copy custom Nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
