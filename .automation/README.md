# SmartEstate Automation Setup

## How It Works

```
GitHub (polling every 60s)
  ├── New issue with label [backend]  → n8n workflow 01 → listener.ps1 → opens Backend Dev Claude session
  ├── New issue with label [frontend] → n8n workflow 02 → listener.ps1 → opens Frontend Dev Claude session
  └── New open PR (no reviewer)       → n8n workflow 03 → listener.ps1 → opens Lead Dev Claude session
```

**De-duplication mechanism:**
- Backend/Frontend issues: n8n only triggers for **unassigned** issues. After triggering, it assigns the issue. So the same issue is never triggered twice.
- PR reviews: n8n only triggers for PRs with **no requested reviewers**. After triggering, it requests a review from the owner.

---

## One-Time Setup

### Step 1 — Start Docker services
```
cd D:\Projects\SmartEstate
docker-compose up -d
```
Services:
- PostgreSQL: `localhost:5432`
- pgAdmin: http://localhost:5050 (admin@smartestate.com / admin)
- n8n: http://localhost:5678 (admin / SmartEstate_n8n_2024!)

### Step 2 — Configure n8n Variables
Open n8n → Settings → Variables → Add:
- `GITHUB_OWNER` = `LazarVujosevic`
- `GITHUB_REPO` = `SmartEstate`

### Step 3 — Configure n8n GitHub Credentials
Open n8n → Credentials → New → GitHub OAuth2 API
- Name it exactly: `SmartEstate GitHub`
- You need a GitHub Personal Access Token (PAT) with scopes: `repo`, `read:org`
- Create PAT at: GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)

### Step 4 — Import n8n Workflows
In n8n UI → Workflows → Import from file:
1. `n8n-workflows/01-backend-issue-trigger.json`
2. `n8n-workflows/02-frontend-issue-trigger.json`
3. `n8n-workflows/03-pr-review-trigger.json`

After importing each workflow, **activate** it (toggle in top right).

### Step 5 — GitHub Issue Labels
In your GitHub repo, create these labels:
- `backend` (color: #0075ca)
- `frontend` (color: #e4e669)
- `architecture` (color: #d93f0b)
- `bug` (color: #d73a4a)

---

## Every Dev Session — Start Listener

Before starting work each day, run the listener (keeps n8n connected to Claude):

**Double-click** `.automation\start-listener.ps1`
OR run in PowerShell:
```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\.automation\start-listener.ps1
```

Keep the "SmartEstate Listener" terminal window open while working.

---

## How Claude Sessions Open

When n8n triggers, the listener opens a **new Windows Terminal tab** named "Claude - backend/frontend/leaddev" in the `D:\Projects\SmartEstate` directory with `claude --dangerously-skip-permissions` already running.

The session receives the issue/PR details and role instructions automatically.

**Important:** Each new Claude session reads CLAUDE.md and its role .md file before starting work.

---

## Troubleshooting

**n8n can't reach listener:**
- Make sure listener.ps1 is running (check for "SmartEstate Listener" window)
- n8n uses `host.docker.internal:9876` to reach Windows host — this should work automatically on Docker Desktop for Windows

**Issue triggered twice:**
- The assign-on-trigger mechanism prevents this. If it happens, manually assign the issue on GitHub.

**PR triggered twice:**
- The request-reviewer mechanism prevents this. If it happens, manually add a reviewer on GitHub.
