// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import starlightLinksValidator from 'starlight-links-validator';
import mermaid from 'astro-mermaid';

// https://astro.build/config
export default defineConfig({
	integrations: [
		// Must come before starlight: it rewrites ```mermaid code blocks before
		// Starlight's syntax highlighting claims them.
		mermaid({
			theme: 'neutral',
			autoTheme: true,
		}),
		starlight({
			title: 'dotnet-affected',
			description:
				'A .NET tool that determines which projects are affected by a set of changes, for large repositories and monorepos.',
			social: [
				{
					icon: 'github',
					label: 'GitHub',
					href: 'https://github.com/leonardochaia/dotnet-affected',
				},
			],
			editLink: {
				baseUrl: 'https://github.com/leonardochaia/dotnet-affected/edit/main/docs/',
			},
			plugins: [starlightLinksValidator()],
			sidebar: [
				{
					label: 'Getting started',
					items: [
						{ label: 'Installation', slug: 'getting-started/installation' },
						{ label: 'Quick start', slug: 'getting-started/quick-start' },
						{ label: 'How it works', slug: 'getting-started/how-it-works' },
					],
				},
				{
					label: 'Guides',
					items: [
						{ label: 'Build and test what changed', slug: 'guides/build-and-test' },
						{ label: 'Comparing commit ranges', slug: 'guides/commit-ranges' },
						{ label: 'Project discovery', slug: 'guides/project-discovery' },
						{ label: 'Output formats', slug: 'guides/output-formats' },
						{ label: 'Excluding projects', slug: 'guides/excluding-projects' },
						{ label: 'Exploring with assumed changes', slug: 'guides/assume-changes' },
						{ label: 'NuGet package changes', slug: 'guides/nuget-packages' },
					],
				},
				{
					label: 'Continuous integration',
					items: [
						{ label: 'Overview', slug: 'ci' },
						{ label: 'GitHub Actions', slug: 'ci/github-actions' },
					],
				},
				{
					label: 'MSBuild SDK',
					items: [
						{ label: 'Overview', slug: 'msbuild-sdk' },
						{ label: 'Filtering by project properties', slug: 'msbuild-sdk/filtering' },
						{ label: 'SDK reference', slug: 'msbuild-sdk/reference' },
						{ label: 'Troubleshooting', slug: 'msbuild-sdk/troubleshooting' },
					],
				},
				{
					label: 'Reference',
					items: [
						{ label: 'CLI', slug: 'reference/cli' },
						{ label: 'Exit codes', slug: 'reference/exit-codes' },
					],
				},
				{
					label: 'Upgrading',
					items: [{ label: 'v6 to v7', slug: 'upgrading/v6-to-v7' }],
				},
				{
					label: 'Contributing',
					items: [{ label: 'Building the project', slug: 'contributing/building' }],
				},
			],
		}),
	],
});
