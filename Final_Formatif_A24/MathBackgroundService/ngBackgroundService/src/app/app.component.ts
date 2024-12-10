import { AccountService } from './services/account.service';
import { Component, NgZone } from '@angular/core';
import { environment } from 'src/environments/environment';

// On doit commencer par ajouter signalr dans les node_modules: npm install @microsoft/signalr
// Ensuite on inclut la librairie
import * as signalR from "@microsoft/signalr"

enum Operation {
  Add,
  Substract,
  Multiply
}

interface MathQuestion {
  operation: Operation;
  valueA: number;
  valueB: number;
  answers: number[];
  playerChoices: number[];
}

interface PlayerInfoDTO {
  nbRightAnswers: number;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'ngBackgroundService';

  baseUrl = environment.apiUrl;

  nbRightAnswers = 0;

  private hubConnection?: signalR.HubConnection

  isConnected = false;
  selection = -1;

  currentQuestion: MathQuestion | null = null;

  constructor(public account: AccountService, private zone: NgZone) {
  }

  SelectChoice(choice: number) {
    this.selection = choice;
    this.hubConnection!.invoke('SelectChoice', choice)
  }

  async register() {
    try {
      await this.account.register();
    }
    catch (e) {
      alert("Erreur pendant l'enregistrement!!!!!");
      return;
    }
    alert("L'enregistrement a été un succès!");
  }

  async login() {
    await this.account.login();
  }

  async logout() {
    await this.account.logout();

    if (this.hubConnection?.state == signalR.HubConnectionState.Connected)
      this.hubConnection.stop();
    this.isConnected = false;
  }

  isLoggedIn(): Boolean {
    return this.account.isLoggedIn();
  }

  connectToHub() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.baseUrl + 'game', { accessTokenFactory: () => sessionStorage.getItem("token")! })
      .build();

    if (!this.hubConnection)
      return;

    this.hubConnection.on('PlayerInfo', (data: PlayerInfoDTO) => {
      this.zone.run(() => {
        console.log(data);
        this.isConnected = true;
        this.nbRightAnswers = data.nbRightAnswers;
      });
    });

    this.hubConnection.on('CurrentQuestion', (data: MathQuestion) => {
      this.zone.run(() => {
        console.log(data);
        this.selection = -1;
        this.currentQuestion = data;
      });
    });

    this.hubConnection.on('IncreasePlayersChoices', (choiceIndex: number) => {
      this.zone.run(() => {
        if (this.currentQuestion) {
          this.currentQuestion.playerChoices[choiceIndex]++;
        }
      });
    });

    // TODO 02 (DONE): [Client] Faire un alert qui affiche « Bonne réponse ! » et qui met à jour nbRightAnswers lorsque le client reçoit un message qui indique une bonne réponse.
    this.hubConnection.on('GoodAnswer', (data: PlayerInfoDTO) => {
      this.zone.run(() => {
        this.nbRightAnswers = data.nbRightAnswers;
        alert('Bonne réponse !');
      });
    });

    // TODO 02 (DONE): [Client] Faire un alert qui affiche « Mauvaise réponse ! La bonne réponse était X » où X est la bonne réponse à la question.
    this.hubConnection.on('BadAnswer', (rightAnswer: number) => {
      this.zone.run(() => {
        alert('Mauvaise réponse! La bonne réponse était ' + rightAnswer + '!');
      });
    });

    this.hubConnection
      .start()
      .then(() => {
        console.log("Connected to Hub");
        // TODO 03 (DONE): [Client] Appeler la méthode GetPlayerInfo du hub pour obtenir le nombre de bonnes réponses du joueur (NbRightAnswers).
        // Cela garantit que les données restent correctes après un rafraîchissement de la page.
        this.hubConnection!.invoke('GetPlayerInfo');
      })
      .catch(err => console.log('Error while starting connection: ' + err))
  }
}
