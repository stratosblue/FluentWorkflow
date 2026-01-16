<script lang="ts">
    import { onMount } from 'svelte';
    import { getAppStatus } from '../../../lib/api';
    import type { WorkerStatusDto } from '../../../lib/types';
    import Link from '../../../components/Link.svelte';
    import Toast from '../../../components/Toast.svelte';
    import Pagination from '../../../components/Pagination.svelte';

    export let params: { appName: string };
    let workers: WorkerStatusDto[] = [];
    let loading = true;
    let error: string | null = null;
    
    // Pagination variables
    let currentPage = 1;
    let totalCount = 0;
    const pageSize = 20;

    async function loadWorkers(page: number = 1) {
        try {
            loading = true;
            error = null;
            const status = await getAppStatus(params.appName, page, pageSize);
            workers = status.items;
            totalCount = Number(status.totalCount) || 0;
            currentPage = page;
        } catch (err) {
            console.error('Failed to load app status:', err);
            error = err instanceof Error ? err.message : '加载失败';
        } finally {
            loading = false;
        }
    }

    onMount(async () => {
        await loadWorkers(currentPage);
    });
</script>

<div class="w-full">
    <div class="flex items-center justify-between mb-8">
        <h1 class="text-3xl font-bold text-gray-800">应用程序: {params.appName}</h1>
        <Link href="#/" class="inline-flex items-center px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-lg text-sm font-medium transition-colors duration-200">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
            </svg>
            返回概览
        </Link>
    </div>

    {#if loading}
        <div class="bg-white rounded-xl shadow-lg p-12 text-center border border-gray-100">
            <div class="inline-block animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
            <p class="mt-4 text-gray-600 font-medium">加载中...</p>
        </div>
    {:else}
        <div class="mb-10">
            <div class="flex items-center justify-between mb-6">
                <h2 class="text-2xl font-semibold text-gray-700">Worker列表</h2>
                <span class="text-sm text-gray-500">共 {totalCount} 个Worker</span>
            </div>
            {#if workers.length > 0}
                <div class="bg-white rounded-xl shadow-md overflow-hidden border border-gray-100">
                    <div class="overflow-x-auto">
                        <table class="min-w-full">
                            <thead class="bg-gray-50 border-b border-gray-200">
                                <tr>
                                    <th class="py-4 px-6 text-left text-sm font-semibold text-gray-600 uppercase tracking-wider">Worker名称</th>
                                    <th class="py-4 px-6 text-left text-sm font-semibold text-gray-600 uppercase tracking-wider">处理中消息数量</th>
                                    <th class="py-4 px-6 text-left text-sm font-semibold text-gray-600 uppercase tracking-wider">状态</th>
                                    <th class="py-4 px-6 text-left text-sm font-semibold text-gray-600 uppercase tracking-wider">操作</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-gray-100">
                                {#each workers as worker}
                                    <tr class="hover:bg-gray-50 transition-colors duration-200 ease-in-out animate-in fade-in duration-300">
                                        <td class="py-4 px-6 text-gray-800 font-medium">{worker.name}</td>
                                        <td class="py-4 px-6 text-gray-600">{worker.processingCount}</td>
                                        <td class="py-4 px-6">
                                            <span class="inline-flex items-center px-3 py-1.5 rounded-full text-xs font-medium transition-all duration-200"
                                                  class:bg-green-100={worker.isActive}
                                                  class:text-green-800={worker.isActive}
                                                  class:bg-red-100={!worker.isActive}
                                                  class:text-red-800={!worker.isActive}>
                                                {#if worker.isActive}
                                                    <span class="h-2 w-2 rounded-full bg-green-500 mr-2 animate-pulse"></span>
                                                    活跃
                                                {:else}
                                                    <span class="h-2 w-2 rounded-full bg-red-500 mr-2"></span>
                                                    非活跃
                                                {/if}
                                            </span>
                                        </td>
                                        <td class="py-4 px-6">
                                            <Link href={`#/workers/${params.appName}/${worker.id}`} class="inline-flex items-center px-3 py-1.5 bg-blue-50 hover:bg-blue-100 text-blue-700 rounded-lg text-sm font-medium transition-all duration-200 shadow-sm hover:shadow">
                                                查看详情
                                                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 ml-1 transition-transform duration-200 group-hover:translate-x-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                                                </svg>
                                            </Link>
                                        </td>
                                    </tr>
                                {/each}
                            </tbody>
                        </table>
                    </div>
                </div>

                <!-- Pagination -->
                <Pagination 
                    {currentPage} 
                    {totalCount} 
                    {pageSize} 
                    onPageChange={loadWorkers} 
                />
            {:else}
                <div class="bg-white rounded-xl shadow-md p-10 text-center border border-gray-100">
                    <div class="text-gray-400 mb-4">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 mx-auto" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                        </svg>
                    </div>
                    <p class="text-gray-500 font-medium">暂无Worker</p>
                    <p class="text-gray-400 text-sm mt-2">此应用程序尚未配置任何Worker</p>
                </div>
            {/if}
        </div>
    {/if}
</div>

{#if error}
    <Toast message={error} type="error" />
{/if}

<script lang="ts" context="module">
export const load = ({ params }) => {
    return {
        title: `应用程序 - ${params.appName}`
    };
};
</script>
