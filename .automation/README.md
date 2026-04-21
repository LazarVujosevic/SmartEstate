# SmartEstate Automation Setup

## How It Works

```
GitHub (polling every 60s)
  ├── New issue with label [backend]  → n8n workflow 01 → listener.ps1 → opens Backend Dev Claude session
  ├── New issue with label [frontend] → n8n workflow 02 → listener.ps1 → opens Frontend Dev Claude session
  └── New open PR (no reviewer)       → n8n workflow 03 → listener.ps1 → opens Lead Dev Claude session
```

**De-duplication mechanism:**
- Backend/Frontend issues: n8n only triggers for **unassigned** issues. After triggering, it assigns the issue — same issue never triggered twice.
- PR reviews: n8n only triggers for PRs with **no requested reviewers**. After triggering, it requests a review from the owner.

---

## One-Time Setup

### Step 1 — Start Docker services
Docker containers start automatically with `docker-compose up -d`.
Services:
- PostgreSQL: `localhost:5432`
- pgAdmin: http://localhost:5050 (admin@smartestate.com / admin)
- n8n: http://localhost:5678 (admin / SmartEstate_n8n_2024!)

### Step 2 — Configure n8n GitHub Credentials
Open n8n → left sidebar icon **Credentials** → **Add credential** → search `GitHub` → select **GitHub API**
- Name it exactly: `SmartEstate GitHub`
- Access Token: your GitHub Personal Access Token (PAT)
- Create PAT at: GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
- Required scopes: `repo` (full), `read:org`

### Step 3 — Import n8n Workflows
In n8n → top-left menu → **Workflows** → **Add workflow** → **Import from file**:
1. `.automation/n8n-workflows/01-backend-issue-trigger.json`
2. `.automation/n8n-workflows/02-frontend-issue-trigger.json`
3. `.automation/n8n-workflows/03-pr-review-trigger.json`

After importing each workflow:
1. Open it
2. Click the **GitHub** nodes and connect them to the `SmartEstate GitHub` credential
3. **Activate** the workflow (toggle top-right, turns green)

### Step 4 — GitHub Issue Labels
In your GitHub repo → Issues → Labels → create:
- `backend` (color: #0075ca)
- `frontend` (color: #e4e669)
- `architecture` (color: #d93f0b)
- `bug` (color: #d73a4a)

---

## Every Dev Session — Start Listener

Before starting work each day, run the listener (bridges n8n → Claude Code):

Right-click `.automation\start-listener.ps1` → **Run with PowerShell**

OR in PowerShell:
```powershell
Set-ExecutionPolicy -Scope Process Bypass
D:\Projects\SmartEstate\.automation\start-listener.ps1
```

Keep the **"SmartEstate Listener"** terminal window open while working. This is what receives n8n calls and opens Claude terminal sessions.

---

## How Claude Sessions Open

When n8n triggers, the listener opens a **new Windows Terminal tab** named `Claude - backend/frontend/leaddev` in `D:\Projects\SmartEstate` with `claude --dangerously-skip-permissions` running. The session receives the issue/PR context automatically in its initial message.

**Each session will:** read `CLAUDE.md` + its role `.md` file, implement the task, create a PR, and update the `.md` files before finishing.

---

## Troubleshooting

**n8n can't reach listener (workflow fails at "Trigger Claude Session" node):**
- Check that `start-listener.ps1` is running (look for "SmartEstate Listener" window)
- n8n reaches the Windows host via `host.docker.internal:9876` — works automatically on Docker Desktop for Windows

**Issue triggered twice:**
- The assign-on-trigger mechanism prevents this. If it happens, manually assign/unassign on GitHub.

**Workflow node shows error about credentials:**
- Open the workflow, click the GitHub node, re-select `SmartEstate GitHub` credential, save.

**GitHub API rate limit:**
- 3 workflows × 1 poll/min = 3 requests/min — well within GitHub's 5000 req/hr limit.
