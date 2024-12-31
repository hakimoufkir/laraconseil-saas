# Stage 1: Build the Angular application
FROM node:20 as build

WORKDIR /app

# Copy package files and install dependencies
COPY package.json package-lock.json ./
RUN npm install

# Copy the Angular project files
COPY . .

# Set the build configuration dynamically (default is production)
ARG CONFIGURATION=production

# Build the Angular application with the specified configuration
RUN npm run build -- --configuration=$CONFIGURATION

# Stage 2: Serve the Angular application with nginx
FROM nginx:alpine

# Copy the Angular build output to nginx's html directory
COPY --from=build /app/dist/subscription-app/browser /usr/share/nginx/html

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Run nginx in the foreground
CMD ["nginx", "-g", "daemon off;"]
