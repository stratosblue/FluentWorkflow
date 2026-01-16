import type { OverviewDto, AppsDetailItemDto, WorkerStatusDto, WorkerDetailDto, MessageDto, MessageListDto, ConsumptionControlDto } from './types';

// Configuration interface
interface ApiConfiguration {
    baseApiAddress: string;
}

// Configuration cache
let apiConfig: ApiConfiguration | null = null;

// Load configuration from configuration.json
async function loadConfig(): Promise<ApiConfiguration> {
    if (!apiConfig) {
        // Get current path to ensure we load configuration from the correct location
        // This handles cases where the app is deployed under a subpath
        const currentPath = window.location.pathname;
        // Remove any hash or query parameters
        const cleanPath = currentPath.split('#')[0].split('?')[0];
        // Get directory path (remove filename if present)
        const dirPath = cleanPath.endsWith('/') ? cleanPath : cleanPath.substring(0, cleanPath.lastIndexOf('/') + 1);
        // Build configuration.json path relative to current location
        const configPath = dirPath + 'configuration.json';
        
        console.log('Loading configuration from:', configPath);
        
        const response = await fetch(configPath);
        
        if (!response.ok) {
            throw new Error(`Failed to load configuration from path: ${configPath}`);
        }
        
        const config = await response.json();
        
        if (!config.baseApiAddress) {
            throw new Error('Configuration file missing required field: baseApiAddress');
        }
        
        apiConfig = {
            baseApiAddress: config.baseApiAddress
        };
    }
    return apiConfig;
}

// Get API base URL with prefix
async function getApiUrl(): Promise<string> {
    const config = await loadConfig();
    return config.baseApiAddress;
}

interface PagedResponse<T> {
    totalCount: number | string | null;
    items: T[];
}

export interface StandardApiResponse<T> {
    code: string;
    data: T | null;
    message: string | null;
}

async function fetchApi<T>(url: string, options: RequestInit = {}): Promise<T> {
    const apiUrl = await getApiUrl();
    // Remove leading slash from url if present to avoid double slashes
    const normalizedUrl = url.startsWith('/') ? url.substring(1) : url;
    const fullUrl = `${apiUrl}/${normalizedUrl}`;
    
    const response = await fetch(fullUrl, {
        ...options,
        headers: {
            'Content-Type': 'application/json',
            ...options.headers,
        },
    });

    if (!response.ok) {
        throw new Error(`API request failed: ${response.status}`);
    }

    const data: StandardApiResponse<T> = await response.json();
    
    // Check if code is not SUCCESS
    if (data.code !== 'SUCCESS') {
        throw new Error(data.message || `API request failed with code: ${data.code}`);
    }

    return data.data as T;
}

export async function getOverview(): Promise<OverviewDto> {
    return fetchApi<OverviewDto>('status/overview');
}

export async function getApps(page: number = 1, pageSize: number = 20): Promise<PagedResponse<AppsDetailItemDto>> {
    return fetchApi<PagedResponse<AppsDetailItemDto>>(`app?page=${page}&pageSize=${pageSize}`);
}

export async function getAppStatus(appName: string, page: number = 1, pageSize: number = 10): Promise<PagedResponse<WorkerStatusDto>> {
    return fetchApi<PagedResponse<WorkerStatusDto>>(`app/status?appName=${appName}&page=${page}&pageSize=${pageSize}`);
}

export async function getWorkerDetail(appName: string, workerId: string): Promise<WorkerDetailDto> {
    return fetchApi<WorkerDetailDto>(`worker?appName=${appName}&workerId=${workerId}`);
}

export async function getWorkerMessages(appName: string, workerId: string, page: number = 1, pageSize: number = 20): Promise<PagedResponse<MessageListDto>> {
    return fetchApi<PagedResponse<MessageListDto>>(`worker/messages?appName=${appName}&workerId=${workerId}&page=${page}&pageSize=${pageSize}`);
}

export async function getWorkerMessageDetail(appName: string, workerId: string, messageId: string, page: number = 1, pageSize: number = 20): Promise<PagedResponse<MessageDto>> {
    return fetchApi<PagedResponse<MessageDto>>(`worker/message-detail?appName=${appName}&workerId=${workerId}&messageId=${messageId}&page=${page}&pageSize=${pageSize}`);
}

export async function postConsumptionControl(dto: ConsumptionControlDto): Promise<boolean> {
    return fetchApi<boolean>('worker/consumption-control', {
        method: 'POST',
        body: JSON.stringify(dto),
    });
}

// Initialize configuration on app startup
export async function initializeConfig(): Promise<void> {
    try {
        await loadConfig();
        console.log('Configuration loaded successfully');
    } catch (error) {
        console.error('Failed to initialize configuration:', error);
        throw error;
    }
}
