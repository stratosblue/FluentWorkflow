<script lang="ts">
    import type { Snippet } from 'svelte'

    import Link from '../components/Link.svelte'
    import { appName } from '../lib/constants'
    import '../app.css'

    type Props = {
        title?: string
        children: Snippet
    }
    let { title, children }: Props = $props()
</script>

<svelte:head>
    <title>{`${title ? title + ' | ' : ''}${appName}`}</title>
</svelte:head>

<nav class="bg-blue-700 shadow-lg px-4 sm:px-6 py-3 sm:py-4">
    <div class="max-w-7xl mx-auto flex items-center justify-between">
        <div class="flex items-center">
            <Link href="#/" class="text-lg sm:text-xl font-bold text-white hover:text-blue-100 transition-colors duration-200">
                {appName}
            </Link>
        </div>
        <div class="flex items-center space-x-3 sm:space-x-4">
            <!-- 可以在这里添加其他导航项 -->
        </div>
    </div>
</nav>

<main class="max-w-7xl mx-auto px-4 sm:px-6 py-6 sm:py-8 animate-in fade-in duration-500">
    {@render children()}
</main>

<style>
    nav {
        position: sticky;
        top: 0;
        z-index: 10;
    }
</style>
