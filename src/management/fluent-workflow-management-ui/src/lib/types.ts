export interface OverviewDto {
    applications: string[];
}

export interface AppsDetailItemDto {
    name: string;
    workerCount: number | string;
}

export interface WorkerStatusDto {
    id: string;
    name: string;
    processingCount: number | string;
    isActive: boolean;
}

export interface WorkerMessageTimeSequenceStatisticsDto {
    timeSpan: string;
    incomingCount: number | string;
    completedCount: number | string;
    averageProcessingTimeSpan: string;
}

export interface WorkerDetailDto {
    id: string;
    name: string;
    hostName: string;
    version: string;
    startupTime: string;
    metadata: Record<string, string>;
    remoteEndPoint: string | null;
    isActive: boolean;
    processingCount: number | string;
    messageStatistics: WorkerMessageTimeSequenceStatisticsDto[];
    lastActive: string | null;
}

export interface MessageDto {
    id: string;
    eventName: string;
    startTime: string;
    metadata: Record<string, string>;
}

export interface MessageListDto {
    id: string;
    eventName: string;
    startTime: string;
}

export interface ConsumptionControlDto {
    appName: string;
    workerId: string;
    messageId: string;
    type: ConsumptionControlType;
    reason: string | null;
}

export type ConsumptionControlType = number;
