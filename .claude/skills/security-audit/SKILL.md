---
name: security-audit
description: Analyzes project for security vulnerabilities and best practices
tags: documentation, security
---

# Security Audit

> **Scope**: Questions from the Application Security Assessment that can be answered by an AI agent through static analysis of code, infrastructure-as-code, CI/CD configs, IAM policies, and cloud/infrastructure configurations.
> **Excluded**: Questions requiring human/organizational context (business severity, team contacts, SLAs, RTO/RPO, legal compliance judgments).

**Legend**
- Questions marked `[AWS]` are AWS-specific. If your application runs on a different cloud or on-premises, use the **Alternative** guidance provided instead.
- All other questions are cloud-agnostic and apply universally.

---

## How to use this file

For each question below, the agent should:
1. Identify the relevant artifacts to inspect (code, configs, IaC, pipeline files, etc.)
2. Analyze them and produce a finding: `PASS`, `FAIL`, or `PARTIAL` with evidence
3. Suggest a remediation if the finding is `FAIL` or `PARTIAL`
4. Create a concise report entry for the question with the finding, evidence, and remediation, and store as security-audit-DATE.md in the /.agentsworkspace/ directory (create it if missing). Do not paste literal secret values as evidence — reference the file/line and redact the actual value.

---

## 1. Risk Assessment

### RA.2 — Sensitive data handling
**Question**: Does your application store or process sensitive information (PII, finance, HR, medical data)?
**How to analyze**: Scan the codebase and database schemas for fields/models containing names, emails, phone numbers, addresses, payment data, health records. Search for GDPR-related terms, data classification tags, privacy annotations.

---

### RA.3 — Internet exposure
**Question**: Does the application have resources directly exposed to the internet (no IP restriction, no environment restriction)?
**How to analyze**: Search for `0.0.0.0/0` or `::/0` ingress rules in any firewall or network configuration. Inspect reverse proxy configs (nginx, HAProxy, Traefik) for publicly bound listeners. Check Kubernetes Ingress or LoadBalancer Service definitions for unauthenticated public exposure.

> **[AWS]** *Additionally on AWS*: Inspect security group rules, API Gateway stage configurations, and CloudFront distributions. Identify resources placed in public subnets without authentication controls.

---

### RA.6 — Security mechanism
**Question**: Is the application itself a security mechanism (e.g. IAM solution, authentication gateway)?
**How to analyze**: Inspect the application's purpose from README, architecture docs, and entry point code. Look for patterns like token issuance, identity federation, or access control enforcement.

---

## 2. Architecture

### ARC.1 — Input/output requirements and API contracts
**Question**: Are input and output requirements clearly defined and enforced (API contracts, type validation, format enforcement)?
**How to analyze**: Check for OpenAPI/Swagger specs, JSON Schema definitions, protobuf contracts. Inspect controllers/handlers for input validation middleware or decorators.

---

### ARC.2 — Platform service limits awareness
**Question**: Are the capacity thresholds and limits of all used platform services researched and handled?

> **[AWS]** *AWS-specific version*: Are AWS soft/hard limits researched and handled for all used AWS services?
> **How to analyze (AWS)**: Scan IaC (Terraform/CloudFormation) for Lambda concurrency settings, SQS message limits, DynamoDB throughput, and API Gateway throttling. Check if reserved concurrency or service quotas are explicitly configured.

**Alternative (non-AWS)**: Inspect IaC and application configuration for capacity settings on equivalent managed services (e.g. message queue throughput, database connection pool limits, function concurrency on GCP/Azure/K8s). Verify that limits are documented and that the application has a defined behavior when limits are reached.

---

### ARC.3 — Dependency outage handling
**Question**: Can the application handle outages of its dependencies (external/internal APIs, event buses, data sources)?
**How to analyze**: Search the codebase for circuit breakers, retry logic with exponential backoff, timeout configurations, fallback handlers, and dead-letter queue or equivalent error queue configurations.

---

### ARC.4 — Principle of least privilege
**Question**: Do all components follow the principle of least privilege in their access policies?
**How to analyze**: Inspect role and permission definitions in IaC. Flag any wildcard permissions (`*`, `all`, `.*`) where narrower scopes are possible. Verify each service only holds the access rights strictly required.

> **[AWS]** *Additionally on AWS*: Inspect IAM role definitions in Terraform, CDK, or CloudFormation. Flag use of `*` in `Action` or `Resource` fields.

**Alternative (non-AWS)**: Check RBAC configs (Kubernetes `Role`/`ClusterRole`, GCP IAM bindings, Azure RBAC assignments, HashiCorp Vault policies). Verify service accounts are scoped to the minimum required permissions.

---

### ARC.5 — Secrets on need-to-know basis
**Question**: Can secrets only be accessed by entities that strictly need them?
**How to analyze**: Audit access policies on secret storage systems. Verify that secrets are scoped per service and per environment, not shared broadly across teams or components.

> **[AWS]** *Additionally on AWS*: Audit IAM policies granting access to AWS Secrets Manager or SSM Parameter Store paths.

**Alternative (non-AWS)**: Check HashiCorp Vault policies, GCP Secret Manager IAM bindings, Azure Key Vault access policies, or equivalent. Verify per-service isolation of secrets.

---

### ARC.6 — Unique credentials per environment
**Question**: Are credentials and secrets different across dev, preview, live, and sandbox environments?
**How to analyze**: Inspect secret storage naming conventions for environment-specific paths or namespaces. Check `.env` files and configuration templates to ensure no secrets are reused across environment boundaries.

> **[AWS]** *Additionally on AWS*: Check Secrets Manager and SSM Parameter Store for environment-scoped paths (e.g. `/app/live/db_password` vs `/app/dev/db_password`).

**Alternative (non-AWS)**: Verify Vault namespaces or secret paths per environment, Kubernetes Secrets scoped per namespace, or environment-specific configuration in GCP/Azure secret managers.

---

### ARC.7 — Authenticated inter-component communication
**Question**: Are all communications between application components (APIs, middleware, data layers) authenticated?
**How to analyze**: Inspect service-to-service call implementations for authentication headers, mutual TLS (mTLS), or signed tokens. Check service mesh policies (Istio, Linkerd) for peer authentication requirements.

> **[AWS]** *Additionally on AWS*: Check for AWS SigV4 signing on internal API calls, API Gateway resource policies, and VPC endpoint policies enforcing authentication.

**Alternative (non-AWS)**: Check Kubernetes NetworkPolicy for allowed traffic between services. Inspect Istio/Linkerd `PeerAuthentication` and `AuthorizationPolicy` resources. Verify internal API tokens or mTLS certificates in non-cloud deployments.

---

### ARC.9 — Network zone placement
**Question**: Is the application deployed in the correct network zones (internal services not publicly reachable, databases in isolated network segments)?
**How to analyze**: Inspect network segmentation definitions in IaC. Verify that public-facing services are placed in DMZ/public zones and that databases or internal services are in private/isolated segments with no direct public ingress.

> **[AWS]** *Additionally on AWS*: Inspect VPC subnet configurations. Verify databases are deployed in private subnets with no public IP or internet gateway route.

**Alternative (non-AWS)**: Check Kubernetes namespace-level NetworkPolicies restricting database access. Inspect GCP VPC or Azure VNET subnet layouts. For on-premises, verify firewall segmentation rules separating tiers.

---

### ARC.10 — Public endpoint protection
**Question**: Are all public-facing endpoints protected by authentication or other security controls?
**How to analyze**: List all routes/endpoints exposed externally. Check for authentication middleware, API key enforcement, or OAuth/JWT validation on each public route.

> **[AWS]** *Additionally on AWS*: Inspect API Gateway stage configurations for Cognito authorizers, Lambda authorizers, or usage plan enforcement. Check ALB listener rules for authentication actions.

**Alternative (non-AWS)**: Check Kubernetes Ingress annotations for authentication (e.g. `nginx.ingress.kubernetes.io/auth-*`). Inspect nginx/HAProxy config for auth modules. Verify Cloudflare Access or equivalent zero-trust rules for public endpoints.

---

### ARC.11 — Server-side access control enforcement
**Question**: Does the application enforce access control on a trusted server-side layer (not client-side)?
**How to analyze**: Inspect authorization logic location in the codebase. Flag any access control implemented in frontend code. Verify backend middleware validates JWT/session tokens before serving protected resources.

---

### ARC.13 — Password hashing
**Question**: Are user passwords hashed using a secure cryptographic function (bcrypt, argon2, scrypt) before storage?
**How to analyze**: Search the codebase for password storage functions. Flag MD5, SHA-1, or unsalted hashes. Verify use of bcrypt/argon2/scrypt with appropriate cost factors.

---

### ARC.14 — Encryption in transit
**Question**: Are all communications between components encrypted in transit (TLS)?
**How to analyze**: Check load balancer and reverse proxy listener configurations for HTTPS/TLS enforcement. Inspect internal service communication configs for TLS settings. Search for `http://` in internal service URLs or disabled TLS verification flags (`verify=False`, `InsecureSkipVerify`, `NODE_TLS_REJECT_UNAUTHORIZED=0`).

---

### ARC.15 — PII presence
**Question**: Does the application store or process Personally Identifiable Information (PII)?
**How to analyze**: Scan database schemas, data models, and API payloads for PII fields (email, phone, name, address, national ID, IP address). Detect GDPR-relevant data flows.

---

### ARC.16 — PII encrypted at rest
**Question**: Is PII data encrypted at rest?
**How to analyze**: Inspect database and storage configurations for encryption-at-rest settings. Verify that encryption keys are managed appropriately (not hardcoded).

> **[AWS]** *Additionally on AWS*: Check RDS, DynamoDB, and S3 encryption settings. Verify KMS key usage for relevant resources.

**Alternative (non-AWS)**: Check PostgreSQL `pg_hba.conf` and tablespace encryption, MongoDB encrypted storage engine settings, GCP Cloud SQL or Azure SQL encryption flags, or Kubernetes persistent volume encryption classes.

---

### ARC.17 — Sensitive data inventory
**Question**: Are all sensitive data types created and processed by the application identified and classified?
**How to analyze**: Search for a data classification table in documentation or code annotations. Scan data handling code to identify undocumented sensitive fields.

---

### ARC.19 — Data retention policy
**Question**: Are sensitive personal data subject to data retention and automatic deletion policies?
**How to analyze**: Check for scheduled deletion jobs, background workers, or database-native TTL mechanisms tied to PII data. Verify deletion is automated and scoped to the right data types.

> **[AWS]** *Additionally on AWS*: Check DynamoDB TTL settings, S3 lifecycle policies, and RDS scheduled maintenance jobs for data cleanup.

**Alternative (non-AWS)**: Check cron jobs or task scheduler configs (k8s CronJob, Celery Beat, pg_cron) for data purge routines. Inspect GCP Firestore TTL policies, Azure Cosmos DB TTL, or MongoDB TTL indexes.

---

### ARC.20 — User data deletion/export
**Question**: Do users have a mechanism to request deletion or export of their personal data (GDPR right to be forgotten)?
**How to analyze**: Search the codebase for endpoints or functions handling data deletion requests (e.g. `DELETE /user`, terms like `GDPR`, `erasure`, `right_to_be_forgotten`, `export`). Verify the flow actually removes/exports all PII from all storage systems.

---

### ARC.22 — Payment information protection
**Question**: If the application handles payment information, is it protected from unauthorized access or leakage?
**How to analyze**: Search for payment-related code (credit card numbers, CVV, Stripe/Adyen tokens). Verify PCI-DSS controls: no raw card numbers stored, tokenization used, payment fields not logged.

---

### ARC.23 — No production data in test environments
**Question**: Is production data excluded from test and development environments?
**How to analyze**: Check data seeding scripts, test fixtures, and database migration files for real PII or production identifiers. Verify CI/CD pipelines do not connect to production databases.

---

### ARC.24 — Environment separation
**Question**: Are production and non-production environments logically or physically separated?
**How to analyze**: Inspect infrastructure definitions for environment boundaries. Flag shared databases, shared secrets, or shared network segments between production and non-production stages.

> **[AWS]** *Additionally on AWS*: Verify that dev/preview stages use separate AWS accounts or at minimum separate VPCs from live. Inspect IaC for cross-environment resource sharing.

**Alternative (non-AWS)**: Check for separate GCP projects, Azure subscriptions, or Kubernetes clusters/namespaces per environment. For on-premises, verify VLAN or firewall-based isolation between staging and production.

---

### ARC.25 — Third-party dependencies
**Question**: Are any new or unknown third-party contractors/SaaS solutions used by this application?
**How to analyze**: Inspect `package.json`, `requirements.txt`, `go.mod`, `pom.xml`, or equivalent for all external dependencies. List SaaS integrations from environment variables and IaC. Flag any not part of a standard approved vendor list.

---

### ARC.26 — Threat modeling
**Question**: Has threat modeling been conducted before releasing to production or before major changes?
**How to analyze**: Search documentation, wiki pages, and repository files for threat models, attack surface analyses, or STRIDE/DREAD assessments. Flag if no evidence is found.

---

## 3. Development

### DEV.1 — Source control and PR process
**Question**: Is source code stored in a version control system and are pull requests linked to issue/change tickets?
**How to analyze**: Check repository settings for branch protection rules. Inspect PR templates and CI checks that enforce ticket references. Verify the repository is active and not archived.

---

### DEV.2 — Automated security testing (SAST)
**Question**: Is automated security testing (SonarCloud, CodeQL, or equivalent SAST tool) integrated into the pipeline?
**How to analyze**: Inspect `.github/workflows`, `Jenkinsfile`, `.gitlab-ci.yml`, or equivalent CI/CD files for SAST tool invocations. Verify findings are treated as quality gates that block merges.

---

### DEV.3 — Snyk integration
**Question**: Is Snyk used to scan the source code and where is it integrated (developer workstation, CI/CD gate)?
**How to analyze**: Search CI/CD pipeline configs for Snyk CLI commands or Snyk GitHub Actions. Check for `.snyk` policy files in the repository.

---

### DEV.4 — Secure and repeatable builds
**Question**: Are application build and deployment processes automated, secure, and repeatable?
**How to analyze**: Verify the existence of CI/CD pipeline definitions. Check for manual deployment steps, hardcoded secrets in build scripts, or non-reproducible build artifacts (e.g. builds that fetch external resources without pinned versions).

---

### DEV.5 — Strong input type validation
**Question**: Is structured data strongly typed and validated against defined schemas (format, length, allowed characters)?
**How to analyze**: Inspect API handler code for input validation libraries (Joi, Pydantic, Bean Validation, Zod, etc.). Check for schema enforcement on incoming requests. Flag endpoints accepting arbitrary input without validation.

---

### DEV.6 — Semantic data validation
**Question**: Is data validated for semantic correctness (e.g. postcode matches suburb, date ranges are coherent)?
**How to analyze**: Search for cross-field validation logic in the codebase. Check if business rules are enforced server-side.

---

### DEV.7 — Input sanitization
**Question**: Does the application sanitize structured and unstructured data to prevent injection attacks?
**How to analyze**: Scan code for HTML/SQL/shell sanitization patterns. Look for parameterized queries, prepared statements, or ORM usage. Flag raw string interpolation in database queries or shell commands.

---

### DEV.8 — Global error handling
**Question**: Is a catch-all error boundary implemented that logs a generic message with a unique ID without leaking internal details?
**How to analyze**: Search the codebase for global exception/error handlers. Verify that error responses do not include stack traces, internal paths, or database errors. Check that a correlation/trace ID is generated and returned.

---

### DEV.9 — No hardcoded credentials
**Question**: Are repositories free of hardcoded credentials, API keys, passwords, and secrets?
**How to analyze**: Scan the repository (including git history) using pattern matching for common secret patterns (cloud provider keys, API tokens, passwords, private keys). Check if tools like `trufflehog` or `gitleaks` are configured. Inspect `.env` files committed to the repo.

> **[AWS]** *Additionally on AWS*: Include patterns for AWS Access Key IDs (`AKIA…`), AWS Secret Access Keys, and session tokens.

---

### DEV.10 — OWASP Top 10 coverage
**Question**: Have OWASP Top 10 risks been addressed during development?
**How to analyze**: Analyze the codebase for the most common OWASP vulnerabilities: injection flaws, broken authentication, sensitive data exposure, XXE, broken access control, security misconfiguration, XSS, insecure deserialization, use of vulnerable components, insufficient logging.

---

### DEV.11 — Dependency vulnerability monitoring
**Question**: Does the build pipeline warn about outdated or vulnerable components?
**How to analyze**: Check repository settings for Dependabot, Renovate, or equivalent configuration. Inspect `.github/dependabot.yml` or equivalent. Verify CI pipeline fails or warns on known CVEs in dependencies.

---

## 4. Operations

### OPS.1 — Production release traceability
**Question**: Are production releases traced and documented (changelogs, release notes, tags)?
**How to analyze**: Check for Git tags on releases, GitHub/GitLab Releases entries, or CI/CD deployment logs. Inspect if a CHANGELOG file is maintained and updated.

---

### OPS.2 — Infrastructure as Code and runbooks
**Question**: Can the application, configuration, and all dependencies be redeployed from automated scripts or runbooks?
**How to analyze**: Verify presence of IaC covering all infrastructure components. Check for documented runbooks. Flag any manually provisioned resources not represented in code.

> **[AWS]** *Additionally on AWS*: Check for Terraform, CDK, or CloudFormation templates. Flag any AWS resources created via the console without corresponding IaC.

**Alternative (non-AWS)**: Check for Terraform, Pulumi, Helm charts, Kubernetes manifests, Ansible playbooks, or Docker Compose files. Verify the full stack can be reproduced from repository contents alone.

---

### OPS.3 — Backup and restore
**Question**: Are regular backups performed and is data restoration tested?
**How to analyze**: Search for backup configuration files, scheduled jobs, or infrastructure definitions enabling automated backups. Check for evidence of restore test procedures in documentation.

> **[AWS]** *Additionally on AWS*: Inspect AWS Backup plans, RDS automated backup retention, S3 versioning, and DynamoDB point-in-time recovery settings.

**Alternative (non-AWS)**: Check pg_dump/mysqldump cron jobs, Velero configurations for Kubernetes, GCP Cloud SQL backups, Azure Database backup policies, or self-hosted backup tool configs (Barman, pgBackRest).

---

### OPS.4 — Backup security
**Question**: Are backups stored securely to prevent theft or corruption?
**How to analyze**: Verify backup destinations have restricted access controls and, if possible, immutable storage policies. Check that backup storage access is separate from production access.

> **[AWS]** *Additionally on AWS*: Verify backup destinations are in separate AWS accounts or use S3 Object Lock (WORM). Audit IAM permissions on backup S3 buckets.

**Alternative (non-AWS)**: Check that backup destinations (remote storage, NAS, GCS, Azure Blob) have access restricted to backup service accounts only. Verify immutability features (GCS Object Hold, Azure Immutable Storage) if available.

---

### OPS.5 — Rate limiting / DDoS protection
**Question**: Does the application or its infrastructure have rate limiting to prevent abuse and DDoS attacks?
**How to analyze**: Search for rate limiting configuration at the reverse proxy, API gateway, or application middleware level. Check for integration with a CDN or DDoS mitigation service.

> **[AWS]** *Additionally on AWS*: Check API Gateway throttling settings, Lambda reserved concurrency limits, AWS Shield Advanced configuration, and WAF rate-based rules.

**Alternative (non-AWS)**: Inspect nginx `limit_req` or `limit_conn` directives, Traefik rate limit middleware, Kubernetes `LimitRange` / HPA settings, or Cloudflare/Fastly rate limiting rules.

---

### OPS.6 — Bot protection
**Question**: Does the public-facing application have bot protection?
**How to analyze**: Check reverse proxy or CDN configurations for bot detection integration. Verify that bot filtering is active on public endpoints.

> **[AWS]** *Additionally on AWS*: Check WAF managed rule groups for bot control, CloudFront configurations, or Datadome integration.

**Alternative (non-AWS)**: Inspect Cloudflare Bot Management, Fastly bot protection rules, nginx-based challenge configurations, or application-level CAPTCHA/fingerprinting middleware.

---

### OPS.10 — Credential rotation
**Question**: Can all credentials (API keys, DB passwords, tokens) be rotated without downtime?
**How to analyze**: Check that the application re-fetches secrets at runtime (not only at startup). Inspect if rotation runbooks or automated rotation policies exist in the secret management system.

> **[AWS]** *Additionally on AWS*: Verify AWS Secrets Manager rotation policies are enabled and test rotation Lambdas are configured. Check application caching TTL on fetched secrets.

**Alternative (non-AWS)**: Check HashiCorp Vault dynamic secrets or lease renewal configs, GCP Secret Manager version management, Azure Key Vault rotation policies, or application-level secret reload mechanisms (e.g. sidecar pattern, config watcher).

---

### OPS.11 — EDR/antivirus agent on servers
**Question**: Are all virtual machines or servers protected by an Endpoint Detection and Response (EDR) or antivirus agent?

> **[AWS]** *AWS-specific version*: Are all EC2 instances (not managed by AWS services) protected by SentinelOne?
> **How to analyze (AWS)**: Inspect EC2 launch configurations, Auto Scaling Group user data scripts, or configuration management scripts for SentinelOne agent installation. Flag instances without the agent.

**Alternative (non-AWS)**: Check configuration management scripts (Ansible, Chef, Puppet, cloud-init) for EDR agent installation (SentinelOne, CrowdStrike, Wazuh, or equivalent) on all VMs. For Kubernetes worker nodes, verify the DaemonSet or node bootstrap script deploys the agent.

---

### OPS.12 — Antivirus scanning for file uploads
**Question**: Is an automated process in place to scan files from untrusted sources with antivirus?
**How to analyze**: Search for file upload handlers in the codebase. Verify integration with an AV scanning service. Check that scanning is triggered before files are processed or made available to other users.

> **[AWS]** *Additionally on AWS*: Check S3 event triggers that invoke scanning Lambdas. Inspect integration with commercial AV or AWS Macie for sensitive data detection.

**Alternative (non-AWS)**: Check for ClamAV integration, ICAP server usage, or commercial AV API calls in file processing pipelines. Verify scanning hooks are triggered on upload regardless of storage backend (local disk, GCS, Azure Blob, MinIO).

---

### OPS.13 — Malware signature updates
**Question**: Are malware signatures updated on a daily basis (for applications that accept file uploads)?
**How to analyze**: Inspect AV configuration for automatic signature update frequency. Check scheduled tasks managing signature updates.

> **[AWS]** *Additionally on AWS*: Check Lambda environment variables or EC2 scheduled tasks (via Systems Manager or cron) managing ClamAV or other AV signature updates.

**Alternative (non-AWS)**: Check cron jobs, Kubernetes CronJobs, or systemd timers running `freshclam` or equivalent signature update commands. Verify the update schedule and last successful run.

---

### OPS.17 — Application monitoring
**Question**: Is monitoring configured to verify the application behaves as expected (business logic monitoring)?
**How to analyze**: Inspect monitoring and alerting configurations. Verify that business-level metrics are tracked (not just infrastructure metrics). Check for alert rules tied to anomalous application behavior.

> **[AWS]** *Additionally on AWS*: Inspect CloudWatch dashboards, alarms, and custom metrics. Check for X-Ray tracing integration.

**Alternative (non-AWS)**: Check Prometheus/Grafana dashboards and AlertManager rules, Datadog monitors, Elastic APM, New Relic, or equivalent observability tooling for application-level alerting.

---

### OPS.18 — Security event logging
**Question**: Does the application log security-relevant events (auth successes/failures, access control failures, input validation failures, deserialization failures)?
**How to analyze**: Scan logging implementation in the codebase. Verify that authentication events, authorization denials, and validation errors are explicitly logged with sufficient context (user ID, IP, timestamp, action).

---

### OPS.19 — No sensitive data in logs
**Question**: Does the application avoid logging sensitive data (PII, secrets, API keys, session tokens)?
**How to analyze**: Scan logging statements in the codebase for PII fields, password parameters, and secret values. Check log output format for data masking or redaction patterns.

---

### OPS.20 — Patch management
**Question**: Are servers and installed applications kept up-to-date with the latest security patches?
**How to analyze**: Check for automated patching configuration or scheduled patch jobs. Inspect base image versions used in containers or VMs and compare against current stable releases.

> **[AWS]** *Additionally on AWS*: Check AWS Systems Manager Patch Manager baseline configurations and compliance reports. Inspect AMI ages and whether they are rebuilt regularly.

**Alternative (non-AWS)**: Check unattended-upgrades or yum-cron configuration on Linux servers. Inspect Ansible patching playbooks or Puppet/Chef patch management modules. For containers, verify base images are rebuilt from up-to-date base OS images in the CI pipeline.

---

### OPS.21 — No shared or default accounts
**Question**: Are shared or default accounts (root, admin, sa, postgres) absent or locked down?
**How to analyze**: Check database user configurations for default accounts. Inspect application user management for shared service accounts. Verify privileged default accounts are disabled or have their default credentials changed.

> **[AWS]** *Additionally on AWS*: Audit IAM user list for accounts named `admin`, `root`, or similar defaults. Verify the AWS root account has MFA enabled and no active access keys.

**Alternative (non-AWS)**: Check GCP IAM for overly broad primitive roles assigned to default service accounts. Inspect Azure Active Directory for shared or default application registrations. For on-premises/k8s, verify no `default` service account has elevated permissions.

---

### OPS.22 — Firewall and network policy hardening
**Question**: Are all network access controls configured with the principle of least privilege (no catch-all allow rules where avoidable)?
**How to analyze**: Inspect firewall rules, network policies, and resource-level access policies in IaC. Flag any rule allowing unrestricted access (`any`/`*`/`0.0.0.0/0`) on sensitive ports without documented justification.

> **[AWS]** *Additionally on AWS*: Inspect all security group rules and NACLs for `0.0.0.0/0` ingress. Audit SQS, OpenSearch, Secrets Manager, and S3 bucket policies for overly permissive wildcard `Action` or `Principal` fields.

**Alternative (non-AWS)**: Check Kubernetes NetworkPolicy resources for default-deny posture and explicit allow rules. Inspect GCP VPC firewall rules or Azure NSG rules for overly permissive entries. For on-premises, audit iptables or pfSense/OPNsense rule sets.

---

### OPS.23 — Infrastructure resource tagging/labeling
**Question**: Are all infrastructure resources tagged or labeled according to a defined convention (team, environment, application)?

> **[AWS]** *AWS-specific version*: Are all AWS resources tagged according to the organization's AWS tagging convention?
> **How to analyze (AWS)**: Inspect Terraform/CDK/CloudFormation for required tags (`team`, `environment`, `application`, `cost-center`). Use AWS Config tag compliance rules to identify untagged resources.

**Alternative (non-AWS)**: Inspect Kubernetes resource labels and annotations for standard labels (`app`, `env`, `team`, `version`). Check Terraform resource blocks for consistent `labels` or `tags` blocks. Verify GCP labels or Azure resource tags are applied in IaC and enforced via policy (OPA/Gatekeeper, Azure Policy).

---

### OPS.24 — Environment parity (preview vs. live)
**Question**: Are the preview and live stage infrastructures equivalent? If not, what are the differences?
**How to analyze**: Compare IaC configurations for preview and live environments side by side. Identify differences in resource types, instance sizes, scaling policies, feature flags, or missing services between the two stages.

> **[AWS]** *Additionally on AWS*: Diff Terraform workspaces or CloudFormation stack parameters between preview and live accounts.

**Alternative (non-AWS)**: Diff Helm values files or Kustomize overlays between staging and production. Compare Terraform variable files across environments. Flag any resource present in production but absent in staging that could mask issues during testing.

---

*End of automated audit checklist — 52 questions, cloud-agnostic.*
