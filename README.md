SMS Rate Limiter Microservice
This microservice acts as a gatekeeper for sending SMS messages, ensuring that messages are sent only when they comply with the provider’s rate limits. Built with .NET Core (C#), it helps avoid unnecessary API calls and associated costs by enforcing limits before dispatching messages.

Overview
The service performs real-time checks to enforce two critical limits:

Per-Phone Number Limit: Maximum messages allowed per business phone number per second.

Per-Account Limit: Maximum messages allowed across the entire account per second.

Designed to handle high volumes of requests, it efficiently manages system resources and automatically cleans up tracking data for inactive numbers.

Features
Rate Limit Enforcement: Prevents sending messages if the defined limits are exceeded.

High-Performance: Optimized to handle large volumes of SMS requests reliably.

Resource Management: Automatically purges tracking for numbers that are no longer active.

Simple Integration: Easily integrates as a pre-check before calling external SMS provider APIs.

Usage
Integrate the microservice into your messaging workflow to determine if an SMS can be sent:

Query the Service: Provide the business phone number to check the current rate.

Evaluate the Response: The service returns whether sending the SMS complies with the set limits.

Proceed Accordingly: If permitted, proceed with your external SMS API call; otherwise, defer the message.

