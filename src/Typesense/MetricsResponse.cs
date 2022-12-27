using System.Text.Json.Serialization;

namespace Typesense;

public sealed record MetricsResponse
{
    [JsonPropertyName("system_cpu1_active_percentage")]
    public string SystemCPU1ActivePercentage { get; init; }

    [JsonPropertyName("system_cpu2_active_percentage")]
    public string SystemCPU2ActivePercentage { get; init; }

    [JsonPropertyName("system_cpu3_active_percentage")]
    public string SystemCPU3ActivePercentage { get; init; }

    [JsonPropertyName("system_cpu4_active_percentage")]
    public string SystemCPU4ActivePercentage { get; init; }

    [JsonPropertyName("system_cpu_active_percentage")]
    public string SystemCPUActivePercentage { get; init; }

    [JsonPropertyName("system_disk_total_bytes")]
    public string SystemDiskTotalBytes { get; init; }

    [JsonPropertyName("system_disk_used_bytes")]
    public string SystemDiskUsedBytes { get; init; }

    [JsonPropertyName("system_memory_total_bytes")]
    public string SystemMemoryTotalBytes { get; init; }

    [JsonPropertyName("system_network_received_bytes")]
    public string SystemNetworkReceivedBytes { get; init; }

    [JsonPropertyName("system_network_sent_bytes")]
    public string SystemNetworkSentBytes { get; init; }

    [JsonPropertyName("typesense_memory_active_bytes")]
    public string TypesenseMemoryActiveBytes { get; init; }

    [JsonPropertyName("typesense_memory_allocated_bytes")]
    public string TypesenseMemoryAllocatedbytes { get; init; }

    [JsonPropertyName("typesense_memory_fragmentation_ratio")]
    public string TypesenseMemoryFragmentationRatio { get; init; }

    [JsonPropertyName("typesense_memory_mapped_bytes")]
    public string TypesenseMemoryMappedBytes { get; init; }

    [JsonPropertyName("typesense_memory_metadata_bytes")]
    public string TypesenseMemoryMetadataBytes { get; init; }

    [JsonPropertyName("typesense_memory_resident_bytes")]
    public string TypesenseMemoryResidentBytes { get; init; }

    [JsonPropertyName("typesense_memory_retained_bytes")]
    public string TypenseMemoryRetainedBytes { get; init; }

    [JsonConstructor]
    public MetricsResponse(
        string systemCPU1ActivePercentage,
        string systemCPU2ActivePercentage,
        string systemCPU3ActivePercentage,
        string systemCPU4ActivePercentage,
        string systemCPUActivePercentage,
        string systemDiskTotalBytes,
        string systemDiskUsedBytes,
        string systemMemoryTotalBytes,
        string systemnetworkReceivedBytes,
        string systemNetworkSentBytes,
        string typesenseMemoryActiveBytes,
        string typesenseMemoryAllocatedbytes,
        string typesenseMemoryFragmentationRatio,
        string typesenseMemoryMappedBytes,
        string typesenseMemoryMetadataBytes,
        string typesenseMemoryResidentBytes,
        string typenseMemoryRetainedBytes)
    {
        SystemCPU1ActivePercentage = systemCPU1ActivePercentage;
        SystemCPU2ActivePercentage = systemCPU2ActivePercentage;
        SystemCPU3ActivePercentage = systemCPU3ActivePercentage;
        SystemCPU4ActivePercentage = systemCPU4ActivePercentage;
        SystemCPUActivePercentage = systemCPUActivePercentage;
        SystemDiskTotalBytes = systemDiskTotalBytes;
        SystemDiskUsedBytes = systemDiskUsedBytes;
        SystemMemoryTotalBytes = systemMemoryTotalBytes;
        SystemNetworkReceivedBytes = systemnetworkReceivedBytes;
        SystemNetworkSentBytes = systemNetworkSentBytes;
        TypesenseMemoryActiveBytes = typesenseMemoryActiveBytes;
        TypesenseMemoryAllocatedbytes = typesenseMemoryAllocatedbytes;
        TypesenseMemoryFragmentationRatio = typesenseMemoryFragmentationRatio;
        TypesenseMemoryMappedBytes = typesenseMemoryMappedBytes;
        TypesenseMemoryMetadataBytes = typesenseMemoryMetadataBytes;
        TypesenseMemoryResidentBytes = typesenseMemoryResidentBytes;
        TypenseMemoryRetainedBytes = typenseMemoryRetainedBytes;
    }
}
