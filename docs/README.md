# dotnet-affected documentation

The documentation site, built with [Astro](https://astro.build) and
[Starlight](https://starlight.astro.build). Pages live in `src/content/docs/`; navigation is configured in
`astro.config.mjs`.

## Working on the site

The site needs Node and pnpm. If you'd rather not install them, run everything in a container — the commands below
mount this repository into `node:22-bookworm` and need nothing on your machine except Docker.

Install dependencies:

```bash
docker run --rm -u "$(id -u):$(id -g)" -e HOME=/tmp/nodehome -e COREPACK_ENABLE_DOWNLOAD_PROMPT=0 \
  -v "$PWD":/work -w /work/docs node:22-bookworm \
  bash -lc 'mkdir -p /tmp/nodehome/bin && corepack enable --install-directory /tmp/nodehome/bin pnpm && export PATH=/tmp/nodehome/bin:$PATH && pnpm install --store-dir=/work/docs/node_modules/.pnpm-store'
```

Run the dev server on <http://localhost:4321>:

```bash
docker run --rm -u "$(id -u):$(id -g)" -e HOME=/tmp/nodehome -e COREPACK_ENABLE_DOWNLOAD_PROMPT=0 \
  -v "$PWD":/work -w /work/docs -p 4321:4321 node:22-bookworm \
  bash -lc 'mkdir -p /tmp/nodehome/bin && corepack enable --install-directory /tmp/nodehome/bin pnpm && export PATH=/tmp/nodehome/bin:$PATH && pnpm run dev --host 0.0.0.0'
```

Build the static site into `dist/` (replace `pnpm run dev --host 0.0.0.0` with `pnpm run build` above).

Run these from the **repository root**, not from `docs/`. `--store-dir` keeps pnpm's content-addressable store inside
`node_modules/`, so it stays on the same filesystem as the install and out of the working tree.

With Node installed locally, the equivalents are the usual `pnpm install`, `pnpm run dev` and `pnpm run build` from
this directory.

## Conventions

- One page per topic under a section directory; the sidebar in `astro.config.mjs` is explicit, so new pages must be
  added there.
- Use Starlight's `<Aside>` (`:::note`, `:::tip`, `:::caution`) for callouts.
- Command output in examples comes from actually running the tool — regenerate it rather than editing it by hand.
- Internal links are checked at build time by
  [`starlight-links-validator`](https://starlight-links-validator.vercel.app/); a broken link fails `pnpm run build`.
