import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '@core/services/auth.service';
import { UserService } from '@services/user.service';
import { ToastService } from '@core/services/toast.service';
import { UserAvatarComponent } from '@shared/components/user-avatar/user-avatar.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [FormsModule, ReactiveFormsModule, UserAvatarComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  public auth = inject(AuthService);
  private userService = inject(UserService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  tab: 'profile' | 'security' = 'profile';
  changingPassword = signal(false);

  fullName = () => {
    const u = this.auth.currentUser();
    return u ? `${u.firstName} ${u.lastName}` : '';
  };
  email = () => this.auth.currentUser()?.email ?? '';
  employeeId = () => this.auth.currentUser()?.employeeId ?? '';
  department = () => this.auth.currentUser()?.departmentName ?? '';
  userRole = () => this.auth.userRole() ?? '';
  managerName = () => this.auth.currentUser()?.managerName ?? '';
  managerEmployeeId = () => this.auth.currentUser()?.managerEmployeeId ?? '';

  isAdminUser = computed(() => {
    const user = this.auth.currentUser();
    const roleFromAuth = this.auth.userRole()?.toLowerCase() || '';
    if (user) {
      const roleId = user.roleId || user.roleID;
      const roleName = (user.roleName || '').toLowerCase().trim();
      if (roleId === 1 || roleName === 'admin' || roleName === 'administrator') return true;
    }
    return roleFromAuth === 'admin' || roleFromAuth === 'administrator' || this.auth.isAdmin();
  });

  passwordForm!: FormGroup;

  passwordMismatch = () => {
    if (!this.passwordForm) return false;
    const np = this.passwordForm.get('newPassword')?.value;
    const cp = this.passwordForm.get('confirmPassword')?.value;
    return np && cp && np !== cp;
  };

  ngOnInit(): void {
    const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;
    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(8), Validators.pattern(passwordPattern)]],
      confirmPassword: ['', Validators.required],
    });
  }

  formError = signal('');

  onChangePassword(): void {
    if (this.passwordMismatch()) return;
    this.passwordForm.markAllAsTouched();
    if (this.passwordForm.invalid) return;

    this.changingPassword.set(true);
    this.formError.set('');

    this.userService.changePassword(this.passwordForm.value).subscribe({
      next: (res) => {
        this.changingPassword.set(false);
        this.toast.success(res.message || 'Password updated successfully.');
        this.passwordForm.reset();
      },
      error: (e: any) => {
        this.changingPassword.set(false);
        // Extract message from response
        let msg = 'Failed to update password.';
        if (e?.error?.errors) {
          msg = Object.values(e.error.errors).flat().join(' ') || msg;
        } else if (e?.error?.message) {
          msg = e.error.message;
        }
        this.formError.set(msg);
        this.toast.error(msg);
      }
    });
  }
}