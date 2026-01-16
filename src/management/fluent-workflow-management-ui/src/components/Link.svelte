<script lang="ts">
    import type { Snippet } from 'svelte'

    type Props = {
        href: string
        class?: string
        children: Snippet
    }

    let { href, children, ...props }: Props = $props()

    let className =
        props.class ||
        'text-blue-600 hover:text-blue-700 underline underline-offset-3 active:text-blue-800 font-medium transition-colors duration-200'
</script>

<a {href} class={className}>
    {@render children()}
</a>
