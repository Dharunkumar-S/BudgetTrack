import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { filter } from 'rxjs';

const ROUTE_TITLES: Record<string, string> = {
  '/dashboard': 'Dashboard',
  '/budgets': 'Budgets',
  '/expenses': 'Expenses',
  '/categories': 'Categories',
  '/departments': 'Departments',
  '/reports': 'Reports & Analytics',
  '/users': 'User Management',
  '/audits': 'Audit Logs',
  '/notifications': 'Notifications',
  '/profile': 'Profile & Settings',
  '/login': 'Sign In',
};

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `<router-outlet />`,
})
export class App implements OnInit {
  private titleService = inject(Title);
  private router = inject(Router);

  ngOnInit(): void {
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe((e: any) => {
      const url: string = e.urlAfterRedirects;
      const match = Object.keys(ROUTE_TITLES).find(k => url.startsWith(k));
      this.titleService.setTitle(match ? `${ROUTE_TITLES[match]} — BudgetTrack` : 'BudgetTrack');
    });
  }
}
