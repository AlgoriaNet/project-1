# Documentation Rules

## What Agents Can Do Freely
- Fix bugs, add features, refactor code
- Update all .md files 
- Add API documentation
- Log implementation decisions

## What Needs Boss Approval
- Changes to user experience/gameplay
- New monetization features
- External service integrations
- Major architecture changes

## Where to Document What

| File | Content | When to Update |
|------|---------|----------------|
| `CLAUDE.md` | Architecture patterns, debugging | Always after major patterns |
| `API_REFERENCE.md` | All APIs, integration examples | After API changes |
| `DEVELOPMENT_HISTORY.md` | Session logs, decisions made | After each session |

## Update Format
```markdown
## YYYY-MM-DD - Agent Name - What Changed
- Brief description
- Files modified
- Why this change
```

Keep it simple. Just document what matters for the next agent.