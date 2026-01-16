<script lang="ts">
    import { onMount } from 'svelte';

    type Props = {
        message: string;
        type?: 'error' | 'success' | 'info';
        duration?: number;
    };

    let { message, type = 'error', duration = 5000 } = $props();
    let isVisible = $state(true);

    onMount(() => {
        // Only auto hide for success and info messages
        if (type !== 'error') {
            const timer = setTimeout(() => {
                isVisible = false;
            }, duration);

            return () => clearTimeout(timer);
        }
    });

    function handleClose() {
        isVisible = false;
    }
</script>

{#if isVisible}
    <div class="fixed bottom-6 right-6 z-50 max-w-md animate-in slide-in-from-bottom-5 fade-in duration-300">
        <div class="bg-white rounded-xl shadow-2xl p-6 border-l-4 border-t border-r border-b"
             class:border-red-500={type === 'error'}
             class:border-green-500={type === 'success'}
             class:border-blue-500={type === 'info'}
             class:border-red-100={type === 'error'}
             class:border-green-100={type === 'success'}
             class:border-blue-100={type === 'info'}>
            <div class="flex items-start">
                <div class="flex-1">
                    <p class="text-sm font-medium"
                       class:text-red-700={type === 'error'}
                       class:text-green-700={type === 'success'}
                       class:text-blue-700={type === 'info'}>
                        {message}
                    </p>
                </div>
                <button 
                    class="ml-4 text-gray-400 hover:text-gray-600 text-lg transition-colors duration-200"
                    onclick={handleClose}
                >
                    ×
                </button>
            </div>
        </div>
    </div>
{/if}
