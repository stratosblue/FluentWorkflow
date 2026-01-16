<script lang="ts">
    import { onMount } from 'svelte';
    import { getApps, initializeConfig } from '../lib/api';
    import type { AppsDetailItemDto } from '../lib/types';
    import Link from '../components/Link.svelte';
    import Toast from '../components/Toast.svelte';
    import Pagination from '../components/Pagination.svelte';

    let appsDetail: AppsDetailItemDto[] = [];
    let loading = true;
    let error: string | null = null;
    
    // Pagination variables
    let currentPage = 1;
    let totalCount = 0;
    const pageSize = 20;

    async function loadApps(page: number = 1) {
        try {
            loading = true;
            error = null;
            
            const apps = await getApps(page, pageSize);
            appsDetail = apps.items;
            totalCount = Number(apps.totalCount) || 0;
            currentPage = page;
        } catch (err) {
            console.error('Failed to load overview:', err);
            error = err instanceof Error ? err.message : '加载失败';
        } finally {
            loading = false;
        }
    }

    onMount(async () => {
        try {
            // Initialize configuration first
            await initializeConfig();
            
            await loadApps(currentPage);
        } catch (err) {
            console.error('Failed to load overview:', err);
            error = err instanceof Error ? err.message : '加载失败';
            loading = false;
        }
    });
</script>

<div class="w-full">
    <h1 class="text-3xl font-bold text-gray-800 mb-8">概览</h1>

    {#if loading}
        <div class="bg-white rounded-xl shadow-lg p-12 text-center border border-gray-100">
            <div class="inline-block animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
            <p class="mt-4 text-gray-600 font-medium">加载中...</p>
        </div>
    {:else}
        <div class="mb-10">
            <h2 class="text-2xl font-semibold text-gray-700 mb-6">应用程序列表</h2>
            {#if appsDetail.length > 0}
                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {#each appsDetail as app}
                        <Link href={`#/apps/${app.name}`} class="block group animate-in fade-in duration-300">
                            <div class="bg-white rounded-xl shadow-md hover:shadow-xl transition-all duration-300 ease-in-out border border-gray-100 p-6 hover:-translate-y-1 hover:border-blue-200">
                                <div class="flex items-center justify-between mb-4">
                                    <div class="text-blue-600 font-bold text-lg">
                                        {app.name}
                                    </div>
                                    <div class="bg-blue-100 text-blue-800 text-xs font-medium px-3 py-1 rounded-full hover:bg-blue-200 transition-colors duration-200">
                                        {app.workerCount} Workers
                                    </div>
                                </div>
                                <p class="text-sm text-gray-500">
                                    管理此应用的所有工作进程
                                </p>
                            </div>
                        </Link>
                    {/each}
                </div>
            {:else}
                <div class="bg-white rounded-xl shadow-md p-10 text-center border border-gray-100">
                    <div class="text-gray-400 mb-4">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-12 w-12 mx-auto" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                        </svg>
                    </div>
                    <p class="text-gray-500 font-medium">暂无应用程序</p>
                    <p class="text-gray-400 text-sm mt-2">系统中尚未配置任何应用程序</p>
                </div>
            {/if}

            <!-- Pagination -->
            <Pagination 
                {currentPage} 
                {totalCount} 
                {pageSize} 
                onPageChange={loadApps} 
            />
        </div>
    {/if}
</div>

{#if error}
    <Toast message={error} type="error" />
{/if}

<script lang="ts" context="module">
export const load = () => {
    return {
        title: '概览'
    };
};
</script>
