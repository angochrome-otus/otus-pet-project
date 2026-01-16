# ?? DEPRECATED: This Dockerfile is no longer used
# 
# The application has been migrated to a microservices architecture.
# Each service has its own Dockerfile in its respective directory:
#
# - SteelDesignerEngineer.ApiGateway/Dockerfile
# - SteelDesignerEngineer.Services.Auth/Dockerfile
# - SteelDesignerEngineer.Services.Session/Dockerfile
# - SteelDesignerEngineer.Services.PageContent/Dockerfile
#
# To build and run the services, use docker-compose:
#   docker-compose -f docker-compose.microservices.yml up -d
#
# Or build individual services:
#   docker build -f SteelDesignerEngineer.ApiGateway/Dockerfile -t steel-api-gateway .
#
# See .github/README.md for more information about CI/CD and deployment.