package com.example.TicketAndNotification.Models.Notification;

import java.time.LocalDateTime;
import java.util.UUID;

import com.example.Shared.Enum.ENotificationType;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.EnumType;
import jakarta.persistence.Enumerated;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity 
@Table(name = "notification")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class NotificationModel {
    @Id 
    @Column(name = "notification_id", nullable = false)
    private UUID notificationId;

    @Column(name = "notification_sender_account_id", nullable = false)
    private UUID notificationSenderAccountId;

    @Enumerated(EnumType.STRING)
    @Column(name = "notification_type", nullable = false)
    private ENotificationType notificationType;

    @Column(name = "notification_content", nullable = false, length = 255)
    private String notificationContent;

    @Column(name = "notification_is_read", nullable = false)
    private Boolean notificationIsRead;

    @Column(name = "notification_created_at")
    private LocalDateTime notificationCreatedAt;

    @Column(name = "notification_read_at")
    private LocalDateTime notificationReadAt;
}
