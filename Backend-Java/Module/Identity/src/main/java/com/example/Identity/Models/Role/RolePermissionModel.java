package com.example.Identity.Models.Role;

import java.io.Serializable;
import java.time.LocalDateTime;
import java.util.UUID;

import jakarta.persistence.Column;
import jakarta.persistence.Embeddable;
import jakarta.persistence.Embedded;
import jakarta.persistence.Entity;
import jakarta.persistence.Table;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.EqualsAndHashCode;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity 
@Table(name = "role_permission")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class RolePermissionModel implements Serializable{
    @Embedded
    private RolePermissionModelId id;

    @Column(name = "assigned_at")
    private LocalDateTime assignedAt;

    @Embeddable 
    @Getter 
    @Setter 
    @NoArgsConstructor 
    @AllArgsConstructor 
    @EqualsAndHashCode 
    public static class RolePermissionModelId implements Serializable{
        @Column(name = "role_id", nullable=false)
        private UUID roleId;

        @Column(name = "permission_id", nullable=false)
        private UUID permissionId;
    }
    
}
