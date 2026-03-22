# ABP Framework — AWS Cloud Migration

A full-stack web application built with **ABP Framework (.NET 9 / Angular / MySQL)**,
used as a realistic workload to explore cloud infrastructure and DevOps practices
on AWS. The focus of this project is **infrastructure and deployment**, not
application feature development.

> Live branch: `aws` | Region: `ap-southeast-1`

## What this project covers

The ABP BookStore app is containerized and deployed to AWS through a dual CI/CD
pipeline (GitHub Actions + Jenkins), with infrastructure managed entirely via
modular CloudFormation stacks.

## Architecture

### AWS infrastructure
![AWS infrastructure](docs/aws-infrastructure.png)

### CI/CD pipeline
![CI/CD pipeline](docs/cicd-pipeline.png)

### Infrastructure stacks (CloudFormation)

| Stack | Resources |
|---|---|
| Network | VPC, multi-AZ public/private subnets, IGW, route tables, NAT instances |
| Security | IAM roles, security groups, Secrets Manager |
| Database | RDS MySQL instance (private subnet) |
| Compute | ECR repository, ECS Cluster, task definitions |
| Delivery | ALB, S3 bucket, CloudFront distribution |
| App service | ECS Fargate service + auto scaling policies |
| Automation | Scheduled Lambda to start/stop RDS & ECS outside working hours |

### CI/CD pipeline

Two pipelines run in parallel for the API and Angular frontend:

- **GitHub Actions** — builds Docker images, pushes to ECR, triggers ECS deployment
- **Jenkins** (self-hosted on Docker, exposed via Cloudflare Tunnel) — handles
  DB migration via a dedicated ECS `DbMigrator` task, then builds and runs the
  API container; sends email alerts on failure

### Key design decisions

**NAT instances over managed NAT Gateway** — replaced AWS NAT Gateway with
[fck-nat](https://github.com/AndrewGuenther/fck-nat), reducing networking costs
by ~85% in staging.

**Separate DbMigrator task** — database schema migrations run as an isolated
ECS task before the API service starts, keeping migrations atomic and auditable.

**Secrets via environment injection** — RDS credentials (`DB_ENDPOINT`,
`DB_USER`, `DB_PASSWORD`) are resolved at deploy time from CloudFormation
outputs and injected into ECS task definitions, avoiding hardcoded values.

**OpenIddict certificate management** — RSA signing and encryption certificates
for the OpenID Connect server are managed as Jenkins credentials and mounted
into the API container at runtime.

## Local development

**Prerequisites:** .NET 9 SDK, Node 18+, Docker
```bash
# Install ABP client-side libs
abp install-libs

# Start all services (API + Angular + MySQL) via Docker Compose
docker-compose up

# Or run DB migrations standalone
dotnet run --project src/Acme.BookStore.DbMigrator
```

## Cost notes

This is a staging environment. The uptime automation stack shuts down RDS and
ECS services outside working hours. Replacing NAT Gateway with fck-nat saves
roughly $30–35/month at low traffic.
