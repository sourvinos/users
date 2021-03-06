import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { AuthGuardService } from 'src/app/helpers/auth-guard.service';
import { AppComponent } from './app.component';
import { ChangePasswordComponent } from './change-password/change-password.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { HttpInterceptor } from './helpers/http-interceptor';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { ProductDetailsComponent } from './products/ui/product-details.component';
import { ProductListComponent } from './products/ui/product-list.component';
import { ProductUpdateComponent } from './products/ui/product-update.component';
import { RegisterComponent } from './register/register.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { MatButtonModule, MatCheckboxModule, MatDialogModule, MatExpansionModule, MatFormFieldModule, MatIconModule, MatInputModule, MatListModule, MatProgressBarModule, MatProgressSpinnerModule, MatSnackBarModule, MAT_LABEL_GLOBAL_OPTIONS, MAT_SNACK_BAR_DEFAULT_OPTIONS, MatAutocompleteModule, MatDivider, MatDividerModule, MatHint } from '@angular/material';

@NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        HomeComponent,
        RegisterComponent,
        LoginComponent,
        ChangePasswordComponent,
        ForgotPasswordComponent,
        ResetPasswordComponent,
        ProductListComponent,
        ProductDetailsComponent,
        ProductUpdateComponent,
    ],
    imports: [
        BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
        BrowserAnimationsModule,
        HttpClientModule,
        FormsModule,
        MatButtonModule,
        MatFormFieldModule,
        ReactiveFormsModule,
        MatInputModule,
        RouterModule.forRoot([
            { path: '', component: HomeComponent, pathMatch: 'full' },
            { path: 'home', redirectTo: '/' },
            { path: 'register', component: RegisterComponent },
            { path: 'login', component: LoginComponent },
            { path: 'forgotPassword', component: ForgotPasswordComponent },
            { path: 'changePassword', component: ChangePasswordComponent, canActivate: [AuthGuardService] },
            { path: 'resetPassword/:email/:token', component: ResetPasswordComponent },
            { path: 'products', component: ProductListComponent, canActivate: [AuthGuardService] },
            { path: 'products/:id', component: ProductDetailsComponent, canActivate: [AuthGuardService] },
            { path: '**', redirectTo: '/' }
        ])
    ],
    providers: [AuthGuardService,
        { provide: HTTP_INTERCEPTORS, useClass: HttpInterceptor, multi: true },
        { provide: MAT_LABEL_GLOBAL_OPTIONS, useValue: { float: 'always' } }],
    bootstrap: [AppComponent]
})

export class AppModule { }
