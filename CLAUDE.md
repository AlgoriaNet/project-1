## Development Best Practices

- Always check with the assets/logs/console.log for console messages, never ask your boss to copy-paste the error message to you
- Unity MCP is another way to get the console message
- When fixing or writing a script, check the console.log to see if your new codes create any Compilation Errors
- If compilation errors exist, fix your codes first before delivering

## CRITICAL RULE: NO WORKAROUNDS - CONFRONT ROOT CAUSES

### Data Consistency Issues
**RULE: When frontend reports data display issues, investigate backend data consistency FIRST!**

**Most "UI bugs" are actually backend data consistency issues:**
- Flickering colors in equipment displays
- Disappearing data after operations
- Stale displays requiring logout/login to refresh
- Inconsistent data showing different values for same item

**Debugging Methodology:**
1. Verify database state directly
2. Test API response consistency across multiple calls
3. Identify race conditions in backend operations
4. Check data refresh timing and transactions

**Common Root Causes:**
- Race conditions where responses are created before data reload
- Missing transactions in multi-step operations
- Stale cache invalidation
- Incorrect order of operations in API handlers

**Example Case Study - Equipment Color Bug:**
- **Symptom**: Equipment showing wrong colors after rank upgrades
- **Wrong Approach**: Create frontend caching to work around stale data
- **Correct Approach**: Report backend race condition where PlayerProfile was created before equipment.reload
- **Result**: Backend fixed race condition, frontend became simpler and more reliable

### Prevention Rules:
1. **NEVER create workarounds for data consistency issues**
2. **Always suspect backend first when data displays incorrectly**
3. **Provide clear evidence with logs showing inconsistent data**
4. **Demand proper backend fixes, not frontend patches**
5. **Remove any existing workarounds once backend is fixed**

### When to Escalate:
- Same equipment/data showing different values at different times
- Data requiring logout/login to display correctly  
- Inconsistent API responses for identical requests
- UI state not matching expected backend state

**Remember: A proper backend fix makes frontend code simpler and more reliable. Workarounds make both sides more complex and hide real problems.**