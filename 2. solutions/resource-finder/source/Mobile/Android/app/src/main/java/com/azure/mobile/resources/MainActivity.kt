package com.azure.mobile.resources

import android.os.Bundle
import com.google.android.material.snackbar.Snackbar
import androidx.appcompat.app.AppCompatActivity
import android.view.Menu
import android.view.MenuItem
import com.azure.core.log.d
import com.microsoft.identity.client.AuthenticationCallback

import kotlinx.android.synthetic.main.activity_main.*
import com.microsoft.identity.client.*
import com.microsoft.identity.client.exception.*
import android.content.Intent


class MainActivity : AppCompatActivity() {

    val scopes = arrayOf("https://graph.microsoft.com/User.Read")
    val msGraphUrl = "https://graph.microsoft.com/v1.0/me"

    /* Azure AD Variables */
    private lateinit var authApp: PublicClientApplication
    private var authResult: IAuthenticationResult? = null

    override fun onCreate(savedInstanceState: Bundle?) {

        super.onCreate(savedInstanceState)

        setContentView(R.layout.activity_main)
        setSupportActionBar(toolbar)

        fab.setOnClickListener { view ->

            Snackbar.make(view, "Replace with your own action", Snackbar.LENGTH_LONG)
                .setAction("Action", null).show()
        }

        /* Configure your app and save state for this activity */
        authApp = PublicClientApplication(
            this.applicationContext,
            R.raw.auth_config
        )

        /* Attempt to get a user and acquireTokenSilent
         * If this fails we do an interactive request
         */
        authApp.getAccounts { accounts ->

            if (accounts.isNotEmpty()) {

                // assume 1 acct right now
                authApp.acquireTokenSilentAsync(scopes, accounts[0], getAuthCallback())

            } else {
                /* No accounts or >1 account */

                authApp.acquireToken(this, scopes, getAuthCallback())
            }
        }


//        /* Attempt to get a user and acquireTokenSilent
//         * If this fails we do an interactive request
//         */
//        try {
//            authApp?.accounts?.let { accts ->
//
//                if (accts.size == 1) {
//                    /* We have 1 account */
//
//                    authApp!!.acquireTokenSilentAsync(scopes, accts[0], getAuthCallback())
//
//                } else {
//
//                    /* We have no account or >1 account */
//
//                    authApp!!.acquireToken(this, scopes, getAuthCallback())
//                }
//            }
//        } catch (e: IndexOutOfBoundsException) {
//
//            d { "Account at this position does not exist: $e" }
//        }
    }

    override fun onCreateOptionsMenu(menu: Menu): Boolean {

        // Inflate the menu; this adds items to the action bar if it is present.
        menuInflater.inflate(R.menu.menu_main, menu)

        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {

        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        return when (item.itemId) {

            R.id.action_settings -> true
            else -> super.onOptionsItemSelected(item)
        }
    }

    /* Handles the redirect from the System Browser */
    override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {

        data?.let {

            authApp.handleInteractiveRequestRedirect(requestCode, resultCode, data)
        }
    }

    /* Callback used in for silent acquireToken calls.
     * Looks if tokens are in the cache (refreshes if necessary and if we don't forceRefresh)
     * else errors that we need to do an interactive request.
     */
    private fun getAuthCallback(): AuthenticationCallback {

        val activity = this

        return object : AuthenticationCallback {

            override fun onSuccess(authenticationResult: IAuthenticationResult) {

                /* Successfully got a token, call graph now */
                d { "Successfully authenticated" }

                /* Store the authResult */
                authResult = authenticationResult

                /* call graph */
                //callGraphAPI()

                /* update the UI to post call graph state */
                // updateSuccessUI()
            }

            override fun onError(exception: MsalException) {

                /* Failed to acquireToken */
                d { "Authentication failed: $exception" }

                when (exception) {

                    is MsalClientException -> {
                        /* Exception inside MSAL, more info inside MsalError.java */
                    }
                    is MsalServiceException -> {
                        /* Exception when communicating with the STS, likely config issue */
                    }
                    is MsalUiRequiredException -> {
                        /* Tokens expired or no session, retry with interactive */

                        authApp.acquireToken(activity, scopes, getAuthCallback())
                    }
                }
            }

            override fun onCancel() {

                /* User canceled the authentication */
                d { "User cancelled login." }
            }
        }
    }

    /* Callback used for interactive request.  If succeeds we use the access
     * token to call the Microsoft Graph. Does not check cache
     */
//    private fun getAuthInteractiveCallback(): AuthenticationCallback {
//
//        return object : AuthenticationCallback {
//
//            override fun onSuccess(authenticationResult: AuthenticationResult) {
//
//                /* Successfully got a token, call graph now */
//                d { "Successfully authenticated" }
//                d { "ID Token: " + authenticationResult.idToken }
//
//                /* Store the auth result */
//                authResult = authenticationResult
//
//                /* call graph */
////                callGraphAPI()
//
//                /* update the UI to post call graph state */
////                updateSuccessUI()
//            }
//
//            override fun onError(exception: MsalException) {
//
//                /* Failed to acquireToken */
//                d { "Authentication failed: $exception" }
//
//                if (exception is MsalClientException) {
//                    /* Exception inside MSAL, more info inside MsalError.java */
//                } else if (exception is MsalServiceException) {
//                    /* Exception when communicating with the STS, likely config issue */
//                }
//            }
//
//            override fun onCancel() {
//
//                /* User canceled the authentication */
//                d { "User cancelled login." }
//            }
//        }
//    }
}