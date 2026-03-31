# ADR-002: TypeScript for All Frontend Vue Files

**Status:** Accepted  
**Date:** 2026-03-31  

## Decision

All Vue SFC files in the audit module use `<script setup lang="ts">` (TypeScript), consistent
with the enterprise template and the existing estimating app.

## Context

Two options were available: TypeScript (TS) or plain JavaScript (JS).

ChatGPT's initial recommendation was to use plain JS for the new audit module while leaving
existing template TS files alone. However, both the enterprise template and the estimating app
(`stronghold-enterprise-estimating`) are 100% TypeScript with strict mode enabled.

## Reasoning

- The entire codebase — enterprise template and estimating app — is TypeScript with strict mode
- NSwag auto-generates a typed TypeScript API client (`webapp/src/apiclient/client.ts`). Using JS in components that consume this client means losing type safety at the integration boundary
- Existing base components (`BaseFormField.vue`, `BaseDataTable.vue`, etc.) define typed props using TypeScript generics — JS files do not benefit from these type definitions
- A mixed TS/JS codebase creates friction: different linting rules, different IDE assistance, different compile-time guarantees
- The developer already has TypeScript configured (tsconfig.json, ESLint TS parser, strict mode)

## Consequences

- All new `.vue` files use `<script setup lang="ts">`
- All new store files are `.ts`
- All new router files are `.ts`
- Vuelidate validators are typed
- Pinia stores use typed state and actions
