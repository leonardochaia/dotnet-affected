import { defineCollection, z } from 'astro:content';
import { docsLoader } from '@astrojs/starlight/loaders';
import { docsSchema } from '@astrojs/starlight/schema';

export const collections = {
	docs: defineCollection({
		loader: docsLoader(),
		schema: docsSchema({
			extend: z.object({
				// Starlight's banner is per page, so a site wide one is a default on the
				// schema. Remove this whole block at the v7 GA release, along with the
				// preview version strings the release checklist in docs/README.md lists.
				banner: z
					.object({ content: z.string() })
					.default({
						content:
							'<strong>v7 is in preview.</strong> Install it with ' +
							'<code>dotnet tool install dotnet-affected --prerelease</code>, and see ' +
							'<a href="/upgrading/v6-to-v7/">what changed since v6</a>.',
					}),
			}),
		}),
	}),
};
