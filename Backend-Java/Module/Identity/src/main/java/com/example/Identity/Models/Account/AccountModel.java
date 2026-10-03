package com.example.Identity.Models.Account;

import java.time.LocalDateTime;
import java.util.UUID;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity 
@Table(name = "account")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class AccountModel {
    @Id 
    @Column(name = "account_id", nullable = false)
    private UUID accountId;

    @Column(name = "account_email", nullable = false, length = 255)
    private String accountEmail;

    @Column(name = "account_password", nullable = false, length = 255)
    private String accountPassword;

    @Column(name = "account_created_at")
    private LocalDateTime accountCreatedAt;

    @Column(name = "account_updated_at")
    private LocalDateTime accountUpdatedAt;

    @Column(name = "account_is_active", nullable = false)
    private Boolean  accountIsActive;
}
