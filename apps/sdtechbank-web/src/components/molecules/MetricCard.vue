<template>
    <SdCard>
        <div>{{ label }}</div>
        <div>{{ value }}</div>
    </SdCard>
</template>

<script setup lang="ts">
defineProps<{
    label: string
    value: string | number
}>()
</script>