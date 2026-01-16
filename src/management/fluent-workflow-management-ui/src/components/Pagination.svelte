<script lang="ts">
export let currentPage: number;
export let totalCount: number;
export let pageSize: number;
export let onPageChange: (page: number) => void;

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

function handlePrevPage() {
    if (currentPage > 1) {
        onPageChange(currentPage - 1);
    }
}

function handleNextPage() {
    const totalPages = Math.ceil(totalCount / pageSize);
    if (currentPage < totalPages) {
        onPageChange(currentPage + 1);
    }
}

function handlePageClick(page: number) {
    if (page !== currentPage) {
        onPageChange(page);
    }
}
</script>

{#if totalCount > 0}
    <div class="mt-8 bg-white p-4 sm:p-6 rounded-xl shadow-sm border border-gray-100">
        <div class="flex flex-col items-center">
            <div class="text-sm text-gray-500 mb-4 text-center">
                共 {totalCount} 条记录，每页 {pageSize} 条
            </div>
            <div class="flex items-center space-x-1 sm:space-x-2 overflow-x-auto pb-2">
                <button
                    class="inline-flex items-center px-3 sm:px-4 py-2 border border-gray-200 text-xs sm:text-sm font-medium rounded-lg shadow-sm text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors duration-200 whitespace-nowrap"
                    disabled={currentPage <= 1}
                    onclick={handlePrevPage}
                >
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-3 sm:h-4 w-3 sm:w-4 mr-1 sm:mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
                    </svg>
                    <span class="hidden sm:inline">上一页</span>
                    <span class="sm:hidden">←</span>
                </button>
                
                <!-- Page numbers -->
                {#each generatePageNumbers() as page}
                    {#if page === -1}
                        <span class="inline-flex items-center px-3 sm:px-4 py-2 border border-gray-200 text-xs sm:text-sm font-medium rounded-lg shadow-sm text-gray-700 bg-white whitespace-nowrap">
                            ...
                        </span>
                    {:else}
                        <button
                            class="inline-flex items-center px-3 sm:px-4 py-2 border border-gray-200 text-xs sm:text-sm font-medium rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors duration-200 whitespace-nowrap"
                            class:bg-blue-50={page === currentPage}
                            class:text-blue-600={page === currentPage}
                            class:border-blue-200={page === currentPage}
                            class:bg-white={page !== currentPage}
                            class:text-gray-700={page !== currentPage}
                            class:hover:bg-gray-50={page !== currentPage}
                            onclick={() => handlePageClick(page)}
                        >
                            {page}
                        </button>
                    {/if}
                {/each}
                
                <button
                    class="inline-flex items-center px-3 sm:px-4 py-2 border border-gray-200 text-xs sm:text-sm font-medium rounded-lg shadow-sm text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors duration-200 whitespace-nowrap"
                    disabled={currentPage >= Math.ceil(totalCount / pageSize)}
                    onclick={handleNextPage}
                >
                    <span class="hidden sm:inline">下一页</span>
                    <span class="sm:hidden">→</span>
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-3 sm:h-4 w-3 sm:w-4 ml-1 sm:ml-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                    </svg>
                </button>
            </div>
        </div>
    </div>
{/if}
