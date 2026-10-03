package com.example.Identity.Models.Account;

import java.io.Serializable;
import java.time.LocalDateTime;
import java.util.UUID;

import jakarta.persistence.Column;
import jakarta.persistence.Embeddable;
import jakarta.persistence.EmbeddedId;
import jakarta.persistence.Entity;
import jakarta.persistence.Table;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.EqualsAndHashCode;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity
@Table(name = "account_role")
@Getter 
@Setter 
@NoArgsConstructor
@AllArgsConstructor 
@Builder 
public class AccountRoleModel implements Serializable{
    @EmbeddedId
    private AccountRoleModelId id;

    @Column(name = "assigned_at", nullable=false)
    private LocalDateTime assignedAt;

    @Embeddable 
    @Getter 
    @Setter 
    @NoArgsConstructor
    @AllArgsConstructor 
    @EqualsAndHashCode 
    public static class AccountRoleModelId implements Serializable{
        @Column(name = "account_id", nullable=false)
        private UUID accountId;

        @Column(name = "role_id", nullable=false)
        private UUID roleId;
    }
}
