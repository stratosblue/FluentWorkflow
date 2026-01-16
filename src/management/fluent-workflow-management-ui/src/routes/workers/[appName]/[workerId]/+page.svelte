<script lang="ts">
    import { onMount } from 'svelte';
    import { getWorkerDetail, getWorkerMessages, getWorkerMessageDetail, postConsumptionControl } from '../../../../lib/api';
    import type { WorkerDetailDto, MessageDto, ConsumptionControlDto, MessageListDto } from '../../../../lib/types';
    import Link from '../../../../components/Link.svelte';
    import Toast from '../../../../components/Toast.svelte';
    import Pagination from '../../../../components/Pagination.svelte';

    export let params: { appName: string; workerId: string };
    let workerDetail: WorkerDetailDto | null = null;
    let messages: MessageListDto[] = [];
    let loading = true;
    let messagesLoading = false;
    let messageDetailLoading = false;
    let error: string | null = null;
    let showControlModal = false;
    let showDetailModal = false;
    let selectedMessageId: string = '';
    let controlReason: string = '';
    let selectedControlType: number | null = null;
    let messagesContainer: HTMLElement | null = null;
    let scrollPosition = 0;
    
    // Pagination variables
    let currentPage = 1;
    let totalCount = 0;
    const pageSize = 20;

    async function loadMessages(page: number = 1) {
        if (!workerDetail?.isActive) return;
        
        try {
            // Save scroll position before loading
            if (messagesContainer) {
                scrollPosition = messagesContainer.scrollTop;
            }
            
            messagesLoading = true;
            error = null;
            
            const messagesData = await getWorkerMessages(params.appName, params.workerId, page, pageSize);
            messages = messagesData.items;
            totalCount = Number(messagesData.totalCount) || 0;
            currentPage = page;
        } catch (err) {
            console.error('Failed to load messages:', err);
            error = err instanceof Error ? err.message : '加载消息失败';
        } finally {
            messagesLoading = false;
            // Restore scroll position after loading
            setTimeout(() => {
                if (messagesContainer) {
                    messagesContainer.scrollTop = scrollPosition;
                }
            }, 100);
        }
    }

    onMount(async () => {
        try {
            loading = true;
            error = null;
            
            // Load worker detail
            workerDetail = await getWorkerDetail(params.appName, params.workerId);
            
            // Only load messages if worker is active
            if (workerDetail?.isActive) {
                await loadMessages(currentPage);
            }
        } catch (err) {
            console.error('Failed to load worker data:', err);
            error = err instanceof Error ? err.message : '加载失败';
        } finally {
            loading = false;
        }
    });

    function openControlModal(messageId: string) {
        selectedMessageId = messageId;
        controlReason = '';
        selectedControlType = null;
        showControlModal = true;
    }

    let currentMessageDetail: MessageDto | null = null;

    async function openDetailModal(messageId: string) {
        selectedMessageId = messageId;
        currentMessageDetail = null;
        showDetailModal = true;
        
        try {
            messageDetailLoading = true;
            error = null;
            
            const detailData = await getWorkerMessageDetail(params.appName, params.workerId, messageId);
            // Only take the first item from the array
            currentMessageDetail = detailData.items[0] || null;
        } catch (err) {
            console.error('Failed to load message detail:', err);
            error = err instanceof Error ? err.message : '加载消息详情失败';
        } finally {
            messageDetailLoading = false;
        }
    }

    async function handleConsumptionControl() {
        if (!selectedMessageId || selectedControlType === null) return;

        try {
            const dto: ConsumptionControlDto = {
                appName: params.appName,
                workerId: params.workerId,
                messageId: selectedMessageId,
                type: Number(selectedControlType),
                reason: controlReason || null
            };

            const result = await postConsumptionControl(dto);
            if (result) {
                // Show success toast
                error = '操作成功';
                showControlModal = false;
                // Remove the message from memory instead of refreshing
                messages = messages.filter(message => message.id !== selectedMessageId);
                // Update total count
                totalCount = Math.max(0, totalCount - 1);
                // Clear error after 3 seconds
                setTimeout(() => {
                    error = null;
                }, 3000);
            } else {
                // Show failure toast
                error = '操作失败';
                // Clear error after 3 seconds
                setTimeout(() => {
                    error = null;
                }, 3000);
            }
        } catch (err) {
            console.error('Failed to control consumption:', err);
            // Show error toast
            error = err instanceof Error ? err.message : '操作失败';
            // Clear error after 3 seconds
            setTimeout(() => {
                error = null;
            }, 3000);
        }
    }

    function generatePageNumbers() {
        const totalPages = Math.ceil(totalCount / pageSize);
        const pageNumbers: number[] = [];
        
        // Always show first page
        if (totalPages > 0) {
            pageNumbers.push(1);
        }
        
        // Show pages around current page
        const startPage = Math.max(2, currentPage - 1);
        const endPage = Math.min(totalPages - 1, currentPage + 1);
        
        // Add ellipsis if needed
        if (startPage > 2) {
            pageNumbers.push(-1); // Ellipsis marker
        }
        
        // Add pages in range
        for (let i = startPage; i <= endPage; i++) {
            pageNumbers.push(i);
        }
        
        // Add ellipsis if needed
        if (endPage < totalPages - 1) {
            pageNumbers.push(-1); // Ellipsis marker
        }
        
        // Always show last page
        if (totalPages > 1) {
            pageNumbers.push(totalPages);
        }
        
        return pageNumbers;
    }
</script>

<div class="w-full">
    <div class="flex items-center justify-between mb-8">
        <h1 class="text-3xl font-bold text-gray-800">Worker详情</h1>
        <Link href={`#/apps/${params.appName}`} class="inline-flex items-center px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-lg text-sm font-medium transition-colors duration-200">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
            </svg>
            返回应用程序
        </Link>
    </div>

    {#if loading}
        <div class="bg-white rounded-xl shadow-lg p-12 text-center border border-gray-100">
            <div class="inline-block animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
            <p class="mt-4 text-gray-600 font-medium">加载中...</p>
        </div>
    {:else if !workerDetail}
        <div class="bg-white rounded-xl shadow-md p-12 text-center border border-gray-100">
            <div class="text-gray-400 mb-4">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 mx-auto" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                </svg>
            </div>
            <p class="text-gray-500 font-medium">Worker不存在</p>
            <p class="text-gray-400 text-sm mt-2">指定的Worker不存在或已被删除</p>
        </div>
    {:else}
        <!-- Worker Details -->
        <div class="mb-10 bg-white rounded-xl shadow-md overflow-hidden border border-gray-100">
            <div class="p-8">
                <h2 class="text-2xl font-semibold text-gray-800 mb-8">基本信息</h2>
                <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-6">
                    <div class="bg-gray-50 p-5 rounded-lg border border-gray-100 transition-all hover:shadow-sm">
                        <p class="text-sm font-medium text-gray-500">Worker名称</p>
                        <p class="mt-2 text-gray-800 font-semibold">{workerDetail.name}</p>
                    </div>
                    <div class="bg-gray-50 p-5 rounded-lg border border-gray-100 transition-all hover:shadow-sm">
                        <p class="text-sm font-medium text-gray-500">主机名称</p>
                        <p class="mt-2 text-gray-700">{workerDetail.hostName}</p>
                    </div>
                    <div class="bg-gray-50 p-5 rounded-lg border border-gray-100 transition-all hover:shadow-sm">
                        <p class="text-sm font-medium text-gray-500">版本</p>
                        <p class="mt-2 text-gray-700">{workerDetail.version}</p>
                    </div>
                    <div class="bg-gray-50 p-5 rounded-lg border border-gray-100 transition-all hover:shadow-sm">
                        <p class="text-sm font-medium text-gray-500">启动时间</p>
                        <p class="mt-2 text-gray-700">{new Date(workerDetail.startupTime).toLocaleString()}</p>
                    </div>
                    <div class="bg-gray-50 p-5 rounded-lg border border-gray-100 transition-all hover:shadow-sm">
                        <p class="text-sm font-medium text-gray-500">远程端点</p>
                        <p class="mt-2 text-gray-700">{workerDetail.remoteEndPoint || '-'}</p>
                    </div>
                    <div class="bg-gray-50 p-5 rounded-lg border border-gray-100 transition-all hover:shadow-sm">
                        <p class="text-sm font-medium text-gray-500">最后活跃时间</p>
                        <p class="mt-2 text-gray-700">{workerDetail.lastActive ? new Date(workerDetail.lastActive).toLocaleString() : '-'}</p>
                    </div>
                    <div class="bg-gray-50 p-5 rounded-lg border border-gray-100 transition-all hover:shadow-sm">
                        <p class="text-sm font-medium text-gray-500">状态</p>
                        <span class="inline-flex items-center px-4 py-2 rounded-full text-sm font-medium mt-2"
                              class:bg-green-100={workerDetail.isActive}
                              class:text-green-800={workerDetail.isActive}
                              class:bg-red-100={!workerDetail.isActive}
                              class:text-red-800={!workerDetail.isActive}>
                            {#if workerDetail.isActive}
                                <span class="h-2 w-2 rounded-full bg-green-500 mr-2"></span>
                                活跃
                            {:else}
                                <span class="h-2 w-2 rounded-full bg-red-500 mr-2"></span>
                                非活跃
                            {/if}
                        </span>
                    </div>
                    <div class="bg-gray-50 p-5 rounded-lg border border-gray-100 transition-all hover:shadow-sm">
                        <p class="text-sm font-medium text-gray-500">处理中消息数量</p>
                        <p class="mt-2 text-gray-700 font-semibold">{workerDetail.processingCount}</p>
                    </div>
                </div>

                {#if Object.keys(workerDetail.metadata).length > 0}
                    <div class="mt-12">
                        <h3 class="text-xl font-semibold text-gray-800 mb-6">元数据</h3>
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                            {#each Object.entries(workerDetail.metadata) as [key, value]}
                                <div class="bg-gray-50 p-5 rounded-lg border border-gray-100 transition-all hover:shadow-sm">
                                    <span class="text-sm font-medium text-gray-500">{key}</span>
                                    <p class="mt-2 text-gray-700 break-all">{value}</p>
                                </div>
                            {/each}
                        </div>
                    </div>
                {/if}

                {#if workerDetail.messageStatistics && workerDetail.messageStatistics.length > 0}
                    <div class="mt-12">
                        <h3 class="text-xl font-semibold text-gray-800 mb-6">消息统计</h3>
                        <div class="overflow-x-auto">
                            <div class="min-w-full bg-white rounded-lg shadow-sm border border-gray-100">
                                <table class="min-w-full">
                                    <thead class="bg-gray-50 border-b border-gray-200">
                                        <tr>
                                            <th class="py-4 px-6 text-left text-sm font-semibold text-gray-600 uppercase tracking-wider">时间跨度</th>
                                            <th class="py-4 px-6 text-left text-sm font-semibold text-gray-600 uppercase tracking-wider">传入消息数</th>
                                            <th class="py-4 px-6 text-left text-sm font-semibold text-gray-600 uppercase tracking-wider">完成消息数</th>
                                            <th class="py-4 px-6 text-left text-sm font-semibold text-gray-600 uppercase tracking-wider">平均处理时长</th>
                                        </tr>
                                    </thead>
                                    <tbody class="divide-y divide-gray-100">
                                        {#each workerDetail.messageStatistics as stat}
                                            <tr class="hover:bg-gray-50 transition-colors duration-150">
                                                <td class="py-4 px-6 text-sm text-gray-700">{stat.timeSpan}</td>
                                                <td class="py-4 px-6 text-sm text-gray-700">{stat.incomingCount}</td>
                                                <td class="py-4 px-6 text-sm text-gray-700">{stat.completedCount}</td>
                                                <td class="py-4 px-6 text-sm text-gray-700">{stat.averageProcessingTimeSpan}</td>
                                            </tr>
                                        {/each}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                {/if}
            </div>
        </div>

        <!-- Messages List -->
        <div class="mb-10" bind:this={messagesContainer}>
            <h2 class="text-2xl font-semibold text-gray-800 mb-8">消息列表</h2>
            {#if !workerDetail?.isActive}
                <div class="bg-white rounded-xl shadow-md p-12 text-center border border-gray-100">
                    <div class="text-gray-400 mb-4">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 mx-auto" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                    </div>
                    <p class="text-gray-500 font-medium">Worker 已离线</p>
                    <p class="text-gray-400 text-sm mt-2">Worker当前处于非活跃状态，无法加载消息</p>
                </div>
            {:else if messagesLoading}
                <div class="bg-white rounded-xl shadow-md p-12 text-center border border-gray-100">
                    <div class="inline-block animate-spin rounded-full h-10 w-10 border-t-2 border-b-2 border-blue-500 mb-4"></div>
                    <p class="text-gray-600 font-medium">加载消息中...</p>
                </div>
            {:else if messages.length > 0}
                <div class="bg-white rounded-xl shadow-md overflow-hidden border border-gray-100">
                    <div class="overflow-x-auto">
                        <table class="min-w-full">
                            <thead class="bg-gray-50 border-b border-gray-200">
                                <tr>
                                    <th class="py-4 px-6 text-left text-sm font-semibold text-gray-600 uppercase tracking-wider">消息ID</th>
                                    <th class="py-4 px-6 text-left text-sm font-semibold text-gray-600 uppercase tracking-wider">事件名称</th>
                                    <th class="py-4 px-6 text-left text-sm font-semibold text-gray-600 uppercase tracking-wider">开始时间</th>
                                    <th class="py-4 px-6 text-left text-sm font-semibold text-gray-600 uppercase tracking-wider">处理时间</th>
                                    <th class="py-4 px-6 text-left text-sm font-semibold text-gray-600 uppercase tracking-wider">操作</th>
                                </tr>
                            </thead>
                            <tbody class="bg-white divide-y divide-gray-100">
                                {#each messages as message}
                                    <tr class="hover:bg-gray-50 transition-colors duration-200 ease-in-out animate-in fade-in duration-300">
                                        <td class="py-4 px-6 text-sm text-gray-700 font-medium">{message.id}</td>
                                        <td class="py-4 px-6 text-sm text-gray-700">{message.eventName}</td>
                                        <td class="py-4 px-6 text-sm text-gray-700">{new Date(message.startTime).toLocaleString()}</td>
                                        <td class="py-4 px-6 text-sm text-gray-700">
                                            {#if message.startTime}
                                                {(() => {
                                                    const startTime = new Date(message.startTime);
                                                    const currentTime = new Date();
                                                    const processingTime = currentTime.getTime() - startTime.getTime();
                                                    
                                                    // Format processing time
                                                    const seconds = Math.floor(processingTime / 1000);
                                                    const minutes = Math.floor(seconds / 60);
                                                    const hours = Math.floor(minutes / 60);
                                                    
                                                    if (hours > 0) {
                                                        return `${hours}h ${minutes % 60}m ${seconds % 60}s`;
                                                    } else if (minutes > 0) {
                                                        return `${minutes}m ${seconds % 60}s`;
                                                    } else {
                                                        return `${seconds}s`;
                                                    }
                                                })()}
                                            {:else}
                                                -  
                                            {/if}
                                        </td>
                                        <td class="py-4 px-6 whitespace-nowrap text-sm font-medium">
                                            <div class="flex space-x-3">
                                                <button 
                                                    class="inline-flex items-center px-4 py-2 bg-blue-50 hover:bg-blue-100 text-blue-700 rounded-lg text-sm font-medium transition-all duration-200 shadow-sm hover:shadow hover:-translate-y-0.5"
                                                    onclick={() => openDetailModal(message.id)}
                                                >
                                                    查看详情
                                                </button>
                                                <button 
                                                    class="inline-flex items-center px-4 py-2 bg-blue-50 hover:bg-blue-100 text-blue-700 rounded-lg text-sm font-medium transition-all duration-200 shadow-sm hover:shadow hover:-translate-y-0.5"
                                                    onclick={() => openControlModal(message.id)}
                                                >
                                                    消费控制
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                {/each}
                            </tbody>
                        </table>
                    </div>
                </div>
            {:else}
                <div class="bg-white rounded-xl shadow-md p-12 text-center border border-gray-100">
                    <div class="text-gray-400 mb-4">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 mx-auto" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z" />
                        </svg>
                    </div>
                    <p class="text-gray-500 font-medium">暂无消息</p>
                    <p class="text-gray-400 text-sm mt-2">当前没有正在处理的消息</p>
                </div>
            {/if}

            <!-- Pagination -->
            {#if workerDetail?.isActive && totalCount > 0}
                <Pagination 
                    {currentPage} 
                    {totalCount} 
                    {pageSize} 
                    onPageChange={loadMessages} 
                />
            {/if}
        </div>
    {/if}

    <!-- Consumption Control Modal -->
    {#if showControlModal}
        <div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div class="bg-white p-8 rounded-xl w-full max-w-md shadow-2xl border border-gray-100">
                <h3 class="text-xl font-semibold text-gray-800 mb-6">消费控制</h3>
                <div class="mb-6">
                    <div class="block text-sm font-medium text-gray-500 mb-2">消息ID</div>
                    <input type="text" value={selectedMessageId} disabled class="w-full p-3 border border-gray-200 rounded-lg bg-gray-50 text-gray-600" />
                </div>
                <div class="mb-6">
                    <div class="block text-sm font-medium text-gray-500 mb-3">操作类型</div>
                    <div class="space-y-3">
                        <div class="flex items-center">
                            <input 
                                type="radio" 
                                id="abort" 
                                name="controlType" 
                                value="1" 
                                bind:group={selectedControlType}
                                class="h-4 w-4 text-blue-600 border-gray-300 focus:ring-blue-500"
                            />
                            <label for="abort" class="ml-3 text-sm text-gray-700">中止工作流程</label>
                        </div>
                        <div class="flex items-center">
                            <input 
                                type="radio" 
                                id="evict" 
                                name="controlType" 
                                value="2" 
                                bind:group={selectedControlType}
                                class="h-4 w-4 text-blue-600 border-gray-300 focus:ring-blue-500"
                            />
                            <label for="evict" class="ml-3 text-sm text-gray-700">驱逐运行中的工作</label>
                        </div>
                    </div>
                </div>
                <div class="mb-6">
                    <div class="block text-sm font-medium text-gray-500 mb-2">原因（可选）</div>
                    <textarea 
                        bind:value={controlReason} 
                        class="w-full p-3 border border-gray-200 rounded-lg text-gray-700"
                        rows={3}
                        placeholder="请输入操作原因"
                    ></textarea>
                </div>
                <div class="flex justify-end space-x-4">
                    <button 
                        class="px-5 py-2.5 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-lg text-sm font-medium transition-colors duration-200"
                        onclick={() => showControlModal = false}
                    >
                        取消
                    </button>
                    <button 
                        class="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors duration-200"
                        class:opacity-50={!selectedControlType}
                        disabled={!selectedControlType}
                        onclick={handleConsumptionControl}
                    >
                        确定
                    </button>
                </div>
            </div>
        </div>
    {/if}

    <!-- Message Detail Modal -->
    {#if showDetailModal}
        <div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <div class="bg-white rounded-xl w-full max-w-5xl shadow-2xl max-h-[95vh] flex flex-col border border-gray-100">
                <!-- Header with close button (fixed) -->
                <div class="p-6 border-b border-gray-200 bg-gray-50 rounded-t-xl">
                    <div class="flex justify-between items-center">
                        <h3 class="text-xl font-semibold text-gray-800">消息详情</h3>
                        <button 
                            class="text-gray-400 hover:text-gray-600 text-xl transition-colors duration-200"
                            onclick={() => showDetailModal = false}
                        >
                            ×
                        </button>
                    </div>
                </div>
                
                <!-- Content area (scrollable) -->
                <div class="p-6 overflow-y-auto flex-grow">
                    <div class="mb-6">
                        <div class="block text-sm font-medium text-gray-500 mb-2">消息ID</div>
                        <input type="text" value={selectedMessageId} disabled class="w-full p-3 border border-gray-200 rounded-lg bg-gray-50 text-gray-600" />
                    </div>
                    {#if messageDetailLoading}
                        <div class="py-12 text-center">
                            <div class="inline-block animate-spin rounded-full h-10 w-10 border-t-2 border-b-2 border-blue-500 mb-4"></div>
                            <p class="text-gray-600 font-medium">加载中...</p>
                        </div>
                    {:else if currentMessageDetail}
                        <div class="space-y-6">
                            <div class="border border-gray-200 p-6 rounded-lg">
                                <div class="space-y-6">
                                    <div>
                                        <p class="text-sm font-medium text-gray-500">事件名称</p>
                                        <p class="mt-2 text-gray-700 font-medium">{currentMessageDetail.eventName}</p>
                                    </div>
                                    <div>
                                        <p class="text-sm font-medium text-gray-500">开始时间</p>
                                        <p class="mt-2 text-gray-700">{new Date(currentMessageDetail.startTime).toLocaleString()}</p>
                                    </div>
                                </div>
                            </div>
                            
                            {#if Object.keys(currentMessageDetail.metadata).length > 0}
                                <div class="border border-gray-200 p-6 rounded-lg">
                                    <h4 class="text-lg font-medium text-gray-800 mb-4">元数据</h4>
                                    <div class="space-y-4">
                                        {#each Object.entries(currentMessageDetail.metadata) as [key, value], index}
                                            <div class="pb-4" class:border-b-gray-100={index < Object.entries(currentMessageDetail.metadata).length - 1}>
                                                <div class="flex flex-col">
                                                    <div class="flex items-start mb-2">
                                                        <span class="text-sm font-medium text-gray-500 w-16">名称:</span>
                                                        <span class="text-sm font-medium text-gray-700 flex-grow">{key}</span>
                                                    </div>
                                                    <div class="flex items-start">
                                                        <span class="text-sm font-medium text-gray-500 w-16">值:</span>
                                                        <textarea 
                                                            value={value} 
                                                            disabled 
                                                            class="w-full p-3 border border-gray-200 rounded-lg bg-gray-50 text-gray-700 whitespace-normal break-all flex-grow"
                                                            rows={Math.max(2, Math.min(10, (value?.length || 0) / 50 + 1))}
                                                        ></textarea>
                                                    </div>
                                                </div>
                                            </div>
                                        {/each}
                                    </div>
                                </div>
                            {/if}
                        </div>
                    {:else}
                        <div class="py-12 text-center">
                            <div class="text-gray-400 mb-4">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 mx-auto" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                                </svg>
                            </div>
                            <p class="text-gray-500 font-medium">暂无消息详情</p>
                            <p class="text-gray-400 text-sm mt-2">无法加载此消息的详细信息</p>
                        </div>
                    {/if}
                </div>
                
                <!-- Footer (fixed) -->
                <div class="p-6 border-t border-gray-200 bg-gray-50 rounded-b-xl">
                    <div class="flex justify-end">
                        <button 
                            class="px-5 py-2.5 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-lg text-sm font-medium transition-colors duration-200"
                            onclick={() => showDetailModal = false}
                        >
                            关闭
                        </button>
                    </div>
                </div>
            </div>
        </div>
    {/if}
</div>

{#if error}
    <Toast message={error} type={error === '操作成功' ? 'success' : 'error'} />
{/if}

<script lang="ts" context="module">
export const load = ({ params }) => {
    return {
        title: `Worker - ${params.workerId}`
    };
};
</script>
