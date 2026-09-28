---
name: safety-check
description: Check for secrets left in the code
tags: security, secrets
---

# Safety Checks

**❌ STOP and WARN if detected:**
- Secrets: `.env*`, `*.key`, `*.pem`, `credentials.json`, `secrets.yaml`, `id_rsa`, `*.p12`, `*.pfx`, `*.cer`
- API Keys: Any `*_API_KEY`, `*_SECRET`, `*_TOKEN` variables with real values (not placeholders like `your-api-key`, `xxx`, `placeholder`)
- Large files: `>10MB` without Git LFS
- Build artifacts: `node_modules/`, `dist/`, `build/`, `__pycache__/`, `*.pyc`, `.venv/`
- Temp files: `.DS_Store`, `thumbs.db`, `*.swp`, `*.tmp`

**API Key Validation:**
Check modified files for patterns like:
```bash
OPENAI_API_KEY=sk-proj-xxxxx  # ❌ Real key detected!
AWS_SECRET_KEY=AKIA...         # ❌ Real key detected!
STRIPE_API_KEY=sk_live_...    # ❌ Real key detected!

# ✅ Acceptable placeholders:
API_KEY=your-api-key-here
SECRET_KEY=placeholder
TOKEN=xxx
API_KEY=<your-key>
SECRET=${YOUR_SECRET}
```

**✅ Verify:**
- `.gitignore` properly configured
- No merge conflicts
- Correct branch (warn if main/master)
- API keys are placeholders only