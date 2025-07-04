# Kiosk Queue Display Research

## Executive Summary

This document compiles research findings on best practices, performance considerations, and implementation strategies for kiosk queue display systems, specifically focusing on displaying current queue positions in real-time environments.

## Key Research Findings

### 1. Real-Time Updates: Polling vs WebSockets

#### Polling Approach (Current Implementation)
- **Pros:**
  - Simple implementation
  - Works with existing RESTful architecture
  - No special infrastructure requirements
  - Reliable fallback mechanism
- **Cons:**
  - Higher server load with many clients
  - Increased network traffic
  - Potential for stale data between polls
  - Higher latency for updates

#### WebSocket Approach
Based on Wolt's engineering blog, they transitioned from polling to WebSockets for order tracking:
- **Performance Improvements:**
  - Reduced server loads significantly
  - Low-latency updates (near real-time)
  - More efficient resource utilization
- **Implementation Considerations:**
  - Requires WebSocket-compatible infrastructure
  - Need for connection management and reconnection logic
  - Backpressure handling for JWT verification
  - Redis Pub/Sub for message relay between services

### 2. Queue Management Best Practices

According to Nielsen Norman Group's research on virtual queues:

#### Key UX Principles:
1. **Transparency:** Always show current position and estimated wait time
2. **Automatic Updates:** Queue status should refresh automatically without user intervention
3. **Clear Visual Indicators:** Use progress bars or visual trackers to show queue movement
4. **Prepare Users:** Inform customers what happens when their turn arrives

#### Recommended Information Display:
- Current position in queue
- Number of people ahead
- Estimated wait time
- Visual progress indicator
- Last update timestamp

### 3. Performance Optimization Strategies

#### Caching Considerations
For kiosk displays showing queue positions:
- **Client-side caching:** Minimal benefit due to real-time nature
- **Server-side caching:** Can reduce database load but requires careful invalidation
- **In-memory caching:** Ideal for current queue state (Redis, in-memory stores)

#### Display Optimization
Research from kiosk implementations suggests:
- Hide mouse cursors using CSS: `cursor: none;`
- Disable zoom and pinch gestures for touch screens
- Remove scrollbars for dedicated displays
- Implement auto-refresh mechanisms
- Consider display sleep detection

### 4. Polling Optimization Strategies

If maintaining polling approach:
1. **Adaptive Polling Intervals:**
   - Shorter intervals during business hours
   - Longer intervals during quiet periods
   - Dynamic adjustment based on queue activity

2. **Smart Polling:**
   - Only poll when display is visible/active
   - Detect screen saver or display sleep
   - Implement exponential backoff on errors

3. **Efficient Data Transfer:**
   - Return only changed data
   - Use lightweight formats (JSON)
   - Implement HTTP caching headers

### 5. Infrastructure Considerations

#### Load Balancing
- Session affinity may be needed for WebSocket connections
- Consider dedicated endpoints for kiosk displays
- Implement rate limiting to prevent abuse

#### Scalability
- Horizontal scaling easier with polling
- WebSockets require sticky sessions or shared state
- Consider hybrid approach: polling for initial load, WebSockets for updates

### 6. Security Considerations

For public kiosk displays:
- Implement IP whitelisting for kiosk locations
- Use read-only API endpoints
- Avoid exposing sensitive customer information
- Consider separate authentication for kiosks
- Implement rate limiting per kiosk

### 7. User Psychology and Wait Times

Research shows:
- Customers overestimate wait times by up to 36%
- Occupied time feels shorter than unoccupied time
- Known wait times reduce anxiety
- Visual progress indicators improve satisfaction

### 8. Alternative Architectures

#### Server-Sent Events (SSE)
- Simpler than WebSockets
- One-way communication (server to client)
- Works over standard HTTP
- Good browser support

#### Long Polling
- Compromise between polling and WebSockets
- Reduces request frequency
- Still uses HTTP infrastructure

## Recommendations for Current Implementation

### Short-term Optimizations
1. **Implement smart polling:**
   - Detect display state (active/sleeping)
   - Adjust polling frequency based on time of day
   - Add exponential backoff for errors

2. **Add visual enhancements:**
   - Progress animation between updates
   - Smooth transitions when position changes
   - Add "last updated" timestamp

3. **Optimize data transfer:**
   - Return minimal data (just position and status)
   - Implement ETag/If-None-Match headers
   - Consider compression for responses

### Long-term Considerations
1. **Evaluate WebSocket implementation:**
   - Pilot with subset of kiosks
   - Measure performance improvements
   - Consider hybrid approach

2. **Implement caching strategy:**
   - Redis for current queue state
   - Reduce database queries
   - Publish queue changes to cache

3. **Monitor and measure:**
   - Track polling frequency per kiosk
   - Monitor server load patterns
   - Measure user engagement with displays

## Implementation Examples

### CSS for Kiosk Displays
```css
/* Hide cursor for kiosk displays */
body {
    cursor: none;
}

/* Disable text selection */
* {
    -webkit-touch-callout: none;
    -webkit-user-select: none;
    -khtml-user-select: none;
    -moz-user-select: none;
    -ms-user-select: none;
    user-select: none;
}

/* Hide scrollbars */
::-webkit-scrollbar {
    display: none;
}

body {
    -ms-overflow-style: none;
    scrollbar-width: none;
}
```

### Smart Polling JavaScript Example
```javascript
class KioskPoller {
    constructor(endpoint, interval = 5000) {
        this.endpoint = endpoint;
        this.baseInterval = interval;
        this.currentInterval = interval;
        this.isActive = true;
    }

    async poll() {
        if (!this.isActive) {
            this.currentInterval = this.baseInterval * 4; // Slower when inactive
        } else {
            this.currentInterval = this.baseInterval;
        }

        try {
            const response = await fetch(this.endpoint);
            const data = await response.json();
            this.updateDisplay(data);
            this.resetInterval();
        } catch (error) {
            this.handleError(error);
            this.currentInterval = Math.min(this.currentInterval * 2, 60000); // Exponential backoff
        }

        setTimeout(() => this.poll(), this.currentInterval);
    }

    detectActivity() {
        // Implement display sleep detection
        // Adjust this.isActive accordingly
    }
}
```

## Conclusion

While the current polling-based implementation is functional and reliable, there are several optimization opportunities available. The choice between polling and real-time updates depends on factors like:
- Number of kiosks
- Frequency of queue updates
- Infrastructure capabilities
- Performance requirements

For immediate improvements, implementing smart polling with activity detection and visual enhancements would provide better user experience without major architectural changes. Long-term migration to WebSockets or SSE could be considered as the system scales.

## References

1. "From Polling to WebSockets: Improving Order Tracking User Experience" - Wolt Engineering Blog
2. "Virtual Queues: 13 Best Practices for Managing the Wait" - Nielsen Norman Group
3. "Comparing Queuing Strategies in Distributed Systems" - fsck.ai
4. "Managing Mac Kiosks" - University of Utah Marriott Library
5. "How to Reduce Waiting Times with Queue Management" - JRNI