# Shared Layout System

## Overview

We've successfully implemented a shared layout system that provides a consistent UI/UX across all applications in the Sports UI monorepo. This eliminates code duplication and ensures maintainability.

## Architecture

### Core Components

The shared layout system is built in the `@sports-ui/ui` library and consists of:

1. **MainLayoutComponent** (`libs/ui/src/components/layout/main-layout.component.ts`)
   - Main container with responsive Material Design sidenav
   - Configurable layout options
   - Responsive breakpoint handling

2. **SidebarComponent** (`libs/ui/src/components/layout/sidebar.component.ts`)
   - Navigation sidebar with hierarchical menu support
   - Role-based menu item visibility
   - Organization selector (when applicable)
   - User profile section

3. **NavbarComponent** (`libs/ui/src/components/layout/navbar.component.ts`)
   - Top navigation bar
   - User menu with profile/settings/logout
   - Notification center
   - Hamburger menu toggle

### Configuration

Each app can customize the layout through a `LayoutConfig` object:

```typescript
interface LayoutConfig {
  appTitle: string;
  appLogo?: string;
  showUserMenu: boolean;
  showNotifications: boolean;
  showSearch: boolean;
  sidenavMode: 'over' | 'push' | 'side';
  sidenavOpened: boolean;
  showFooter?: boolean;
}
```

### Navigation Items

Apps define their navigation structure using the `NavItem` interface:

```typescript
interface NavItem {
  name: string;
  icon?: string;
  route?: string;
  children?: NavItem[];
  permission?: string;
  role?: string;
  roles?: string[];
}
```

## Implementation

### Apps Using Shared Layout

1. **Sports UI** (`apps/sports-ui`)
   - End-user application
   - Player options, roster management, code redemption
   - Navigation focused on user actions

2. **Sports Admin** (`apps/sport-admin`)
   - Administrative portal
   - User management, system settings, content management
   - Admin-focused navigation structure

3. **GM Portal** (`apps/src`)
   - General Manager application
   - Team management, analytics, organization settings
   - GM-specific workflow navigation

### Usage Example

```typescript
@Component({
  template: `
    <ui-main-layout 
      [navItems]="navItems"
      [config]="layoutConfig"
      [permissionChecker]="checkPermission"
      [currentUser]="currentUser"
      [organizations]="organizations"
      [selectedOrganization]="selectedOrganization"
    ></ui-main-layout>
  `
})
export class AppComponent {
  layoutConfig = {
    appTitle: 'My App',
    showUserMenu: true,
    showNotifications: true,
    sidenavMode: 'side' as const,
    sidenavOpened: true,
  };

  navItems: NavItem[] = [
    { name: 'Dashboard', icon: 'dashboard', route: '/' },
    { 
      name: 'Management', 
      icon: 'settings',
      children: [
        { name: 'Users', icon: 'people', route: '/users' },
        { name: 'Settings', icon: 'tune', route: '/settings' }
      ]
    }
  ];

  checkPermission = (item: NavItem): boolean => {
    // Implement permission logic
    return true;
  };
}
```

## Features

### Responsive Design
- **Mobile**: Overlay sidenav, touch-friendly navigation
- **Tablet**: Push sidenav, optimized spacing
- **Desktop**: Side sidenav, full feature set

### Role-Based Navigation
- Menu items can be restricted by permissions or roles
- Dynamic navigation based on user context
- Hierarchical menu support

### Customizable Branding
- App-specific titles and logos
- Configurable color schemes (future enhancement)
- Flexible layout options

### User Experience
- Consistent navigation patterns
- Responsive breakpoints
- Accessibility support
- Material Design components

## Benefits

✅ **Single Source of Truth**: Layout logic centralized in UI library  
✅ **Consistent Design**: All apps share the same look and feel  
✅ **Easy Maintenance**: Update layout once, affects all apps  
✅ **Customizable**: Each app can have unique navigation and branding  
✅ **Role-Based**: Navigation adapts to user permissions  
✅ **Responsive**: Works seamlessly across all device sizes  
✅ **Scalable**: Easy to add new apps or modify existing ones  

## Development

### Building
```bash
# Build individual apps
npx nx build sports-ui
npx nx build sport-admin

# Build all apps
npx nx run-many --target=build --all
```

### Serving
```bash
# Serve sports-ui
npx nx serve sports-ui

# Serve admin portal
npx nx serve sport-admin
```

### Testing
```bash
# Test UI library
npx nx test ui

# Test all projects
npx nx run-many --target=test --all
```

## Future Enhancements

- [ ] Theme system integration
- [ ] Advanced permission management
- [ ] Real-time notifications
- [ ] Search functionality
- [ ] Breadcrumb navigation
- [ ] Keyboard shortcuts
- [ ] Dark mode support
- [ ] Internationalization (i18n)

## File Structure

```
libs/ui/src/components/layout/
├── main-layout.component.ts
├── main-layout.component.html
├── main-layout.component.css
├── sidebar.component.ts
├── sidebar.component.html
├── sidebar.component.css
├── navbar.component.ts
├── navbar.component.html
└── navbar.component.css
```

This shared layout system provides a solid foundation for consistent, maintainable, and scalable application development across the entire Sports UI ecosystem.
